using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetFlow.Data;
using FleetFlow.Models;

namespace FleetFlow.Controllers
{
    [Authorize(Roles = "FleetManager,Dispatcher")]
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            var vehicles = await _context.Vehicles
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return View(vehicles);
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.Trips.OrderByDescending(t => t.CreatedAt).Take(10))
                .ThenInclude(t => t.Driver)
                .Include(v => v.MaintenanceLogs.OrderByDescending(m => m.ServiceDate).Take(10))
                .Include(v => v.FuelLogs.OrderByDescending(f => f.Date).Take(10))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // GET: Vehicles/Create
        [Authorize(Roles = "FleetManager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FleetManager")] // ADD THIS
        public async Task<IActionResult> Create([Bind("Name,LicensePlate,Type,MaxLoadCapacity,CurrentOdometer,Region,AcquisitionCost")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate license plate
                if (await _context.Vehicles.AnyAsync(v => v.LicensePlate == vehicle.LicensePlate))
                {
                    ModelState.AddModelError("LicensePlate", "This license plate already exists.");
                    return View(vehicle);
                }

                vehicle.Status = VehicleStatus.Available;
                vehicle.CreatedAt = DateTime.Now;
                vehicle.IsRetired = false;

                _context.Add(vehicle);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Vehicle added successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Debug: Show validation errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                TempData["Error"] = error.ErrorMessage;
            }

            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        [Authorize(Roles = "FleetManager")] // ADD THIS
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FleetManager")] // ADD THIS
        public async Task<IActionResult> Edit(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            // Remove validation for fields we don't want to validate
            ModelState.Remove("Trips");
            ModelState.Remove("MaintenanceLogs");
            ModelState.Remove("FuelLogs");

            if (ModelState.IsValid)
            {
                try
                {
                    // FIXED: Get existing vehicle and update properties manually
                    var existingVehicle = await _context.Vehicles.FindAsync(id);

                    if (existingVehicle == null)
                    {
                        return NotFound();
                    }

                    // Update only the fields we want to change
                    existingVehicle.Name = vehicle.Name;
                    existingVehicle.Type = vehicle.Type;
                    existingVehicle.MaxLoadCapacity = vehicle.MaxLoadCapacity;
                    existingVehicle.CurrentOdometer = vehicle.CurrentOdometer;
                    existingVehicle.Status = vehicle.Status;
                    existingVehicle.Region = vehicle.Region;
                    existingVehicle.IsRetired = vehicle.IsRetired;
                    existingVehicle.AcquisitionCost = vehicle.AcquisitionCost;
                    // Note: LicensePlate and CreatedAt should not be changed

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Vehicle updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Debug: Show validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = string.Join(", ", errors);

            return View(vehicle);
        }

        // POST: Vehicles/ToggleRetire/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FleetManager")] // ADD THIS
        public async Task<IActionResult> ToggleRetire(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            vehicle.IsRetired = !vehicle.IsRetired;
            vehicle.Status = vehicle.IsRetired ? VehicleStatus.OutOfService : VehicleStatus.Available;

            await _context.SaveChangesAsync();

            TempData["Success"] = vehicle.IsRetired ? "Vehicle retired successfully!" : "Vehicle reactivated successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
