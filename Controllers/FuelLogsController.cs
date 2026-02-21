using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FleetFlow.Data;
using FleetFlow.Models;

namespace FleetFlow.Controllers
{
    [Authorize]
    public class FuelLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FuelLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FuelLogs
        public async Task<IActionResult> Index(int? vehicleId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.FuelLogs
                .Include(f => f.Vehicle)
                .Include(f => f.Trip)
                .AsQueryable();

            if (vehicleId.HasValue)
            {
                query = query.Where(f => f.VehicleId == vehicleId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(f => f.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(f => f.Date <= toDate.Value);
            }

            var logs = await query
                .OrderByDescending(f => f.Date)
                .ToListAsync();

            // Calculate totals
            ViewBag.TotalLiters = logs.Sum(f => f.Liters);
            ViewBag.TotalCost = logs.Sum(f => f.Cost);

            await PopulateVehicleDropdown(vehicleId);
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(logs);
        }

        // GET: FuelLogs/Create
        public async Task<IActionResult> Create(int? vehicleId, int? tripId)
        {
            await PopulateDropdowns(vehicleId, tripId);

            var model = new FuelLog
            {
                Date = DateTime.Today
            };

            if (vehicleId.HasValue)
            {
                model.VehicleId = vehicleId.Value;
                var vehicle = await _context.Vehicles.FindAsync(vehicleId.Value);
                if (vehicle != null)
                {
                    model.OdometerReading = vehicle.CurrentOdometer;
                }
            }

            if (tripId.HasValue)
            {
                model.TripId = tripId.Value;
            }

            return View(model);
        }

        // POST: FuelLogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleId,TripId,Liters,Cost,Date,OdometerReading,Station")] FuelLog fuelLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fuelLog);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Fuel log added successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(fuelLog.VehicleId, fuelLog.TripId);
            return View(fuelLog);
        }

        // GET: FuelLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var fuelLog = await _context.FuelLogs.FindAsync(id);
            if (fuelLog == null) return NotFound();

            await PopulateDropdowns(fuelLog.VehicleId, fuelLog.TripId);
            return View(fuelLog);
        }

        // POST: FuelLogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,TripId,Liters,Cost,Date,OdometerReading,Station")] FuelLog fuelLog)
        {
            if (id != fuelLog.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fuelLog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fuel log updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FuelLogExists(fuelLog.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(fuelLog.VehicleId, fuelLog.TripId);
            return View(fuelLog);
        }

        // POST: FuelLogs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);
            if (fuelLog == null) return NotFound();

            _context.FuelLogs.Remove(fuelLog);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fuel log deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(int? vehicleId = null, int? tripId = null)
        {
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsRetired)
                .OrderBy(v => v.Name)
                .ToListAsync();

            ViewData["VehicleId"] = new SelectList(vehicles, "Id", "Name", vehicleId);

            var trips = await _context.Trips
                .Where(t => t.Status == TripStatus.Completed || t.Status == TripStatus.Dispatched)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new { t.Id, Display = t.TripCode + " - " + t.Vehicle.Name })
                .ToListAsync();

            ViewData["TripId"] = new SelectList(trips, "Id", "Display", tripId);
        }

        private async Task PopulateVehicleDropdown(int? vehicleId = null)
        {
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsRetired)
                .OrderBy(v => v.Name)
                .ToListAsync();

            ViewData["VehicleId"] = new SelectList(vehicles, "Id", "Name", vehicleId);
        }

        private bool FuelLogExists(int id)
        {
            return _context.FuelLogs.Any(e => e.Id == id);
        }
    }
}
