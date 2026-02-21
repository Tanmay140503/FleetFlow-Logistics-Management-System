using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetFlow.Data;
using FleetFlow.Models;
using FleetFlow.ViewModels;

namespace FleetFlow.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(VehicleType? type, VehicleStatus? status, string region)
        {
            // KPI Calculations
            var activeFleet = await _context.Vehicles
                .CountAsync(v => v.Status == VehicleStatus.OnTrip && !v.IsRetired);

            var maintenanceAlerts = await _context.Vehicles
                .CountAsync(v => v.Status == VehicleStatus.InShop);

            var totalVehicles = await _context.Vehicles
                .CountAsync(v => !v.IsRetired);

            var utilizationRate = totalVehicles > 0
                ? (decimal)activeFleet / totalVehicles * 100
                : 0;

            var pendingCargo = await _context.Trips
                .CountAsync(t => t.Status == TripStatus.Draft);

            var totalDrivers = await _context.Drivers.CountAsync();

            var completedTripsToday = await _context.Trips
                .CountAsync(t => t.Status == TripStatus.Completed &&
                                 t.CompletedAt.HasValue &&
                                 t.CompletedAt.Value.Date == DateTime.Today);

            var todayRevenue = await _context.Trips
                .Where(t => t.Status == TripStatus.Completed &&
                           t.CompletedAt.HasValue &&
                           t.CompletedAt.Value.Date == DateTime.Today)
                .SumAsync(t => t.Revenue ?? 0);

            // Create ViewModel
            var dashboardData = new DashboardViewModel
            {
                ActiveFleet = activeFleet,
                MaintenanceAlerts = maintenanceAlerts,
                UtilizationRate = utilizationRate,
                PendingCargo = pendingCargo,
                TotalVehicles = totalVehicles,
                TotalDrivers = totalDrivers,
                CompletedTripsToday = completedTripsToday,
                TodayRevenue = todayRevenue
            };

            ViewBag.DashboardData = dashboardData;

            // Filtered Vehicles
            var vehiclesQuery = _context.Vehicles
                .Where(v => !v.IsRetired);

            if (type.HasValue)
                vehiclesQuery = vehiclesQuery.Where(v => v.Type == type.Value);

            if (status.HasValue)
                vehiclesQuery = vehiclesQuery.Where(v => v.Status == status.Value);

            if (!string.IsNullOrEmpty(region))
                vehiclesQuery = vehiclesQuery.Where(v => v.Region.Contains(region));

            var vehicles = await vehiclesQuery
                .OrderBy(v => v.Status)
                .ThenBy(v => v.Name)
                .ToListAsync();

            return View(vehicles);
        }
    }
}
