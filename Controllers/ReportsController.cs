using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetFlow.Data;
using FleetFlow.Models;
using FleetFlow.ViewModels;
using ClosedXML.Excel;

namespace FleetFlow.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            // Summary stats
            var totalVehicles = await _context.Vehicles.CountAsync(v => !v.IsRetired);
            var totalDrivers = await _context.Drivers.CountAsync();
            var completedTrips = await _context.Trips.CountAsync(t => t.Status == TripStatus.Completed);
            var totalRevenue = await _context.Trips.Where(t => t.Status == TripStatus.Completed).SumAsync(t => t.Revenue ?? 0);
            var totalFuelCost = await _context.FuelLogs.SumAsync(f => f.Cost);
            var totalMaintenanceCost = await _context.MaintenanceLogs.SumAsync(m => m.Cost);

            ViewBag.TotalVehicles = totalVehicles;
            ViewBag.TotalDrivers = totalDrivers;
            ViewBag.CompletedTrips = completedTrips;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalFuelCost = totalFuelCost;
            ViewBag.TotalMaintenanceCost = totalMaintenanceCost;
            ViewBag.TotalOperationalCost = totalFuelCost + totalMaintenanceCost;
            ViewBag.NetProfit = totalRevenue - (totalFuelCost + totalMaintenanceCost);

            return View();
        }

        // GET: Reports/VehiclePerformance
        public async Task<IActionResult> VehiclePerformance()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Trips)
                .Include(v => v.FuelLogs)
                .Include(v => v.MaintenanceLogs)
                .Where(v => !v.IsRetired)
                .ToListAsync();

            var report = vehicles.Select(v => new VehicleReportViewModel
            {
                VehicleId = v.Id,
                VehicleName = v.Name,
                LicensePlate = v.LicensePlate,
                TotalTrips = v.Trips.Count(t => t.Status == TripStatus.Completed),
                TotalDistance = v.Trips
                    .Where(t => t.EndOdometer.HasValue)
                    .Sum(t => t.EndOdometer.Value - t.StartOdometer),
                TotalFuelCost = v.FuelLogs.Sum(f => f.Cost),
                TotalMaintenanceCost = v.MaintenanceLogs.Sum(m => m.Cost),
                TotalRevenue = v.Trips.Sum(t => t.Revenue ?? 0),
                FuelEfficiency = v.FuelLogs.Sum(f => f.Liters) > 0
                    ? v.Trips.Where(t => t.EndOdometer.HasValue).Sum(t => t.EndOdometer.Value - t.StartOdometer) / v.FuelLogs.Sum(f => f.Liters)
                    : 0,
                ROI = v.AcquisitionCost.HasValue && v.AcquisitionCost.Value > 0
                    ? ((v.Trips.Sum(t => t.Revenue ?? 0) - (v.MaintenanceLogs.Sum(m => m.Cost) + v.FuelLogs.Sum(f => f.Cost))) / v.AcquisitionCost.Value) * 100
                    : 0
            }).ToList();

            return View(report);
        }

        // GET: Reports/DriverPerformance
        public async Task<IActionResult> DriverPerformance()
        {
            var drivers = await _context.Drivers
                .Include(d => d.Trips)
                .ToListAsync();

            var report = drivers.Select(d => new DriverPerformanceViewModel
            {
                DriverId = d.Id,
                DriverName = d.Name,
                TotalTrips = d.Trips.Count,
                CompletedTrips = d.Trips.Count(t => t.Status == TripStatus.Completed),
                CompletionRate = d.Trips.Count > 0
                    ? (decimal)d.Trips.Count(t => t.Status == TripStatus.Completed) / d.Trips.Count * 100
                    : 0,
                SafetyScore = d.SafetyScore,
                TotalDistance = d.Trips
                    .Where(t => t.EndOdometer.HasValue)
                    .Sum(t => t.EndOdometer.Value - t.StartOdometer)
            }).OrderByDescending(d => d.SafetyScore).ToList();

            return View(report);
        }

        // GET: Reports/MonthlyReport
        public async Task<IActionResult> MonthlyReport(int? year, int? month)
        {
            year ??= DateTime.Now.Year;
            month ??= DateTime.Now.Month;

            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var trips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.CompletedAt.HasValue &&
                           t.CompletedAt.Value >= startDate &&
                           t.CompletedAt.Value <= endDate)
                .ToListAsync();

            var fuelLogs = await _context.FuelLogs
                .Where(f => f.Date >= startDate && f.Date <= endDate)
                .ToListAsync();

            var maintenanceLogs = await _context.MaintenanceLogs
                .Where(m => m.ServiceDate >= startDate && m.ServiceDate <= endDate)
                .ToListAsync();

            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.MonthName = startDate.ToString("MMMM yyyy");
            ViewBag.Trips = trips;
            ViewBag.FuelLogs = fuelLogs;
            ViewBag.MaintenanceLogs = maintenanceLogs;
            ViewBag.TotalRevenue = trips.Sum(t => t.Revenue ?? 0);
            ViewBag.TotalFuelCost = fuelLogs.Sum(f => f.Cost);
            ViewBag.TotalMaintenanceCost = maintenanceLogs.Sum(m => m.Cost);

            return View();
        }

        // GET: Reports/ExportVehicleReport
        public async Task<IActionResult> ExportVehicleReport()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Trips)
                .Include(v => v.FuelLogs)
                .Include(v => v.MaintenanceLogs)
                .Where(v => !v.IsRetired)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Vehicle Report");

                // Headers
                worksheet.Cell(1, 1).Value = "Vehicle Name";
                worksheet.Cell(1, 2).Value = "License Plate";
                worksheet.Cell(1, 3).Value = "Type";
                worksheet.Cell(1, 4).Value = "Total Trips";
                worksheet.Cell(1, 5).Value = "Total Distance (km)";
                worksheet.Cell(1, 6).Value = "Fuel Cost";
                worksheet.Cell(1, 7).Value = "Maintenance Cost";
                worksheet.Cell(1, 8).Value = "Revenue";
                worksheet.Cell(1, 9).Value = "Net Profit";

                // Style headers
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

                // Data
                int row = 2;
                foreach (var v in vehicles)
                {
                    var fuelCost = v.FuelLogs.Sum(f => f.Cost);
                    var maintenanceCost = v.MaintenanceLogs.Sum(m => m.Cost);
                    var revenue = v.Trips.Sum(t => t.Revenue ?? 0);
                    var distance = v.Trips
                        .Where(t => t.EndOdometer.HasValue)
                        .Sum(t => t.EndOdometer.Value - t.StartOdometer);

                    worksheet.Cell(row, 1).Value = v.Name;
                    worksheet.Cell(row, 2).Value = v.LicensePlate;
                    worksheet.Cell(row, 3).Value = v.Type.ToString();
                    worksheet.Cell(row, 4).Value = v.Trips.Count(t => t.Status == TripStatus.Completed);
                    worksheet.Cell(row, 5).Value = distance;
                    worksheet.Cell(row, 6).Value = fuelCost;
                    worksheet.Cell(row, 7).Value = maintenanceCost;
                    worksheet.Cell(row, 8).Value = revenue;
                    worksheet.Cell(row, 9).Value = revenue - fuelCost - maintenanceCost;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"VehicleReport_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        // GET: Reports/ExportDriverReport
        public async Task<IActionResult> ExportDriverReport()
        {
            var drivers = await _context.Drivers
                .Include(d => d.Trips)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Driver Report");

                // Headers
                worksheet.Cell(1, 1).Value = "Driver Name";
                worksheet.Cell(1, 2).Value = "Email";
                worksheet.Cell(1, 3).Value = "License Number";
                worksheet.Cell(1, 4).Value = "License Expiry";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "Safety Score";
                worksheet.Cell(1, 7).Value = "Total Trips";
                worksheet.Cell(1, 8).Value = "Completed Trips";
                worksheet.Cell(1, 9).Value = "Completion Rate (%)";

                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGreen;

                int row = 2;
                foreach (var d in drivers)
                {
                    var completedTrips = d.Trips.Count(t => t.Status == TripStatus.Completed);
                    var completionRate = d.Trips.Count > 0
                        ? (decimal)completedTrips / d.Trips.Count * 100
                        : 0;

                    worksheet.Cell(row, 1).Value = d.Name;
                    worksheet.Cell(row, 2).Value = d.Email;
                    worksheet.Cell(row, 3).Value = d.LicenseNumber;
                    worksheet.Cell(row, 4).Value = d.LicenseExpiry.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 5).Value = d.Status.ToString();
                    worksheet.Cell(row, 6).Value = d.SafetyScore;
                    worksheet.Cell(row, 7).Value = d.Trips.Count;
                    worksheet.Cell(row, 8).Value = completedTrips;
                    worksheet.Cell(row, 9).Value = Math.Round(completionRate, 2);
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"DriverReport_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }
    }
}
