using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FleetFlow.Data;
using FleetFlow.Models;

namespace FleetFlow.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Maintenance
        public async Task<IActionResult> Index(bool? showCompleted)
        {
            var query = _context.MaintenanceLogs
                .Include(m => m.Vehicle)
                .AsQueryable();

            if (showCompleted == true)
            {
                // Show all
            }
            else
            {
                // Show only pending
                query = query.Where(m => !m.IsCompleted);
            }

            var logs = await query
                .OrderByDescending(m => m.ServiceDate)
                .ToListAsync();

            ViewBag.ShowCompleted = showCompleted ?? false;
            return View(logs);
        }

        // GET: Maintenance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceLog = await _context.MaintenanceLogs
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (maintenanceLog == null) return NotFound();

            return View(maintenanceLog);
        }

        // GET: Maintenance/Create
        public async Task<IActionResult> Create(int? vehicleId)
        {
            await PopulateVehicleDropdown(vehicleId);

            var model = new MaintenanceLog
            {
                ServiceDate = DateTime.Today
            };

            if (vehicleId.HasValue)
            {
                model.VehicleId = vehicleId.Value;
                var vehicle = await _context.Vehicles.FindAsync(vehicleId.Value);
                if (vehicle != null)
                {
                    model.OdometerAtService = vehicle.CurrentOdometer;
                }
            }

            return View(model);
        }

        // POST: Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleId,ServiceType,Description,Cost,ServiceDate,OdometerAtService,PerformedBy")] MaintenanceLog log)
        {
            if (ModelState.IsValid)
            {
                log.IsCompleted = false;

                // Auto-update vehicle status to InShop
                var vehicle = await _context.Vehicles.FindAsync(log.VehicleId);
                if (vehicle != null)
                {
                    // Check if vehicle is on trip
                    if (vehicle.Status == VehicleStatus.OnTrip)
                    {
                        TempData["Error"] = "Cannot add maintenance for a vehicle that is currently on trip.";
                        await PopulateVehicleDropdown(log.VehicleId);
                        return View(log);
                    }

                    vehicle.Status = VehicleStatus.InShop;
                }

                _context.Add(log);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Maintenance log added. Vehicle status changed to 'In Shop'.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateVehicleDropdown(log.VehicleId);
            return View(log);
        }

        // GET: Maintenance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceLog = await _context.MaintenanceLogs.FindAsync(id);
            if (maintenanceLog == null) return NotFound();

            await PopulateVehicleDropdown(maintenanceLog.VehicleId);
            return View(maintenanceLog);
        }

        // POST: Maintenance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,ServiceType,Description,Cost,ServiceDate,OdometerAtService,PerformedBy,IsCompleted,CompletedDate")] MaintenanceLog maintenanceLog)
        {
            if (id != maintenanceLog.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintenanceLog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Maintenance log updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceLogExists(maintenanceLog.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateVehicleDropdown(maintenanceLog.VehicleId);
            return View(maintenanceLog);
        }

        // POST: Maintenance/MarkComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var log = await _context.MaintenanceLogs
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (log == null) return NotFound();

            log.IsCompleted = true;
            log.CompletedDate = DateTime.Now;

            // Check if there are other pending maintenance logs for this vehicle
            var hasPendingMaintenance = await _context.MaintenanceLogs
                .AnyAsync(m => m.VehicleId == log.VehicleId &&
                              m.Id != log.Id &&
                              !m.IsCompleted);

            // Return vehicle to Available status only if no other pending maintenance
            if (!hasPendingMaintenance)
            {
                log.Vehicle.Status = VehicleStatus.Available;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Maintenance marked as complete.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Maintenance/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var log = await _context.MaintenanceLogs
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (log == null) return NotFound();

            _context.MaintenanceLogs.Remove(log);

            // Check if there are other pending maintenance logs for this vehicle
            var hasPendingMaintenance = await _context.MaintenanceLogs
                .AnyAsync(m => m.VehicleId == log.VehicleId &&
                              m.Id != log.Id &&
                              !m.IsCompleted);

            // Return vehicle to Available status only if no other pending maintenance
            if (!hasPendingMaintenance && log.Vehicle.Status == VehicleStatus.InShop)
            {
                log.Vehicle.Status = VehicleStatus.Available;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Maintenance log deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateVehicleDropdown(int? selectedVehicleId = null)
        {
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsRetired)
                .OrderBy(v => v.Name)
                .ToListAsync();

            ViewData["VehicleId"] = new SelectList(vehicles, "Id", "Name", selectedVehicleId);
        }

        private bool MaintenanceLogExists(int id)
        {
            return _context.MaintenanceLogs.Any(e => e.Id == id);
        }
    }
}
