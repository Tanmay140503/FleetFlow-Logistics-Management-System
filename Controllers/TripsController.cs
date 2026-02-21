
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FleetFlow.Data;
using FleetFlow.Models;

namespace FleetFlow.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trips
        public async Task<IActionResult> Index(TripStatus? status)
        {
            var query = _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var trips = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentFilter = status;
            return View(trips);
        }

        // GET: Trips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trip == null) return NotFound();

            return View(trip);
        }

        // GET: Trips/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleId,DriverId,OriginAddress,DestinationAddress,CargoWeight,CargoDescription,Revenue")] Trip trip)
        {
            // ✅ Remove validation for auto-generated and navigation properties
            ModelState.Remove("TripCode");
            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");
            ModelState.Remove("Notes");
            ModelState.Remove("CargoDescription");

            if (ModelState.IsValid)
            {
                var vehicle = await _context.Vehicles.FindAsync(trip.VehicleId);
                var driver = await _context.Drivers.FindAsync(trip.DriverId);

                if (vehicle == null)
                {
                    ModelState.AddModelError("VehicleId", "Selected vehicle not found.");
                    await PopulateDropdowns();
                    return View(trip);
                }

                if (driver == null)
                {
                    ModelState.AddModelError("DriverId", "Selected driver not found.");
                    await PopulateDropdowns();
                    return View(trip);
                }

                // Validation: Cargo weight must not exceed capacity
                if (trip.CargoWeight > vehicle.MaxLoadCapacity)
                {
                    ModelState.AddModelError("CargoWeight",
                        $"Cargo weight ({trip.CargoWeight}kg) exceeds vehicle capacity ({vehicle.MaxLoadCapacity}kg)");

                    await PopulateDropdowns();
                    return View(trip);
                }

                // Validation: Driver license must be valid
                if (driver.LicenseExpiry <= DateTime.Now)
                {
                    ModelState.AddModelError("DriverId", "Driver's license has expired. Cannot assign this driver.");
                    await PopulateDropdowns();
                    return View(trip);
                }

                // Validation: Check driver license category matches vehicle type
                var vehicleTypeStr = vehicle.Type.ToString();
                if (!driver.LicenseCategory.Contains(vehicleTypeStr))
                {
                    ModelState.AddModelError("DriverId",
                        $"Driver's license category ({driver.LicenseCategory}) doesn't match vehicle type ({vehicleTypeStr})");
                    await PopulateDropdowns();
                    return View(trip);
                }

                // ✅ Set auto-generated values
                trip.TripCode = GenerateTripCode();
                trip.Status = TripStatus.Draft;
                trip.CreatedAt = DateTime.Now;
                trip.StartOdometer = vehicle.CurrentOdometer;

                _context.Add(trip);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Trip {trip.TripCode} created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Debug: Show validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = "Validation failed: " + string.Join(", ", errors);

            await PopulateDropdowns();
            return View(trip);
        }

        // POST: Trips/Dispatch/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dispatch(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            if (trip.Status != TripStatus.Draft)
            {
                TempData["Error"] = "Only draft trips can be dispatched.";
                return RedirectToAction(nameof(Index));
            }

            // Re-validate before dispatch
            if (trip.Vehicle.Status != VehicleStatus.Available)
            {
                TempData["Error"] = "Vehicle is no longer available.";
                return RedirectToAction(nameof(Index));
            }

            if (trip.Driver.LicenseExpiry <= DateTime.Now)
            {
                TempData["Error"] = "Driver's license has expired.";
                return RedirectToAction(nameof(Index));
            }

            trip.Status = TripStatus.Dispatched;
            trip.DispatchedAt = DateTime.Now;

            // Update vehicle and driver status
            trip.Vehicle.Status = VehicleStatus.OnTrip;
            trip.Driver.Status = DriverStatus.OnDuty;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Trip {trip.TripCode} dispatched successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Trips/Complete/5
        public async Task<IActionResult> Complete(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            if (trip.Status != TripStatus.Dispatched)
            {
                TempData["Error"] = "Only dispatched trips can be completed.";
                return RedirectToAction(nameof(Index));
            }

            return View(trip);
        }

        // POST: Trips/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, decimal endOdometer, string? notes, decimal? actualRevenue)
        {
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            // Validation: End odometer must be >= start odometer
            if (endOdometer < trip.StartOdometer)
            {
                TempData["Error"] = "End odometer cannot be less than start odometer.";
                return View(trip);
            }

            trip.Status = TripStatus.Completed;
            trip.CompletedAt = DateTime.Now;
            trip.EndOdometer = endOdometer;
            trip.Notes = notes;

            if (actualRevenue.HasValue)
            {
                trip.Revenue = actualRevenue.Value;
            }

            // Update vehicle status and odometer
            trip.Vehicle.Status = VehicleStatus.Available;
            trip.Vehicle.CurrentOdometer = endOdometer;

            // Update driver status and stats
            trip.Driver.Status = DriverStatus.OffDuty;
            trip.Driver.TotalTripsCompleted++;

            // Update safety score (simple logic)
            if (trip.Driver.SafetyScore < 100)
            {
                trip.Driver.SafetyScore = Math.Min(100, trip.Driver.SafetyScore + 0.5m);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Trip {trip.TripCode} completed successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Trips/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
            {
                TempData["Error"] = "Cannot cancel a completed or already cancelled trip.";
                return RedirectToAction(nameof(Index));
            }

            trip.Status = TripStatus.Cancelled;
            trip.Notes = string.IsNullOrEmpty(reason) ? "Cancelled" : $"Cancelled: {reason}";

            // Release resources if trip was dispatched
            if (trip.Vehicle.Status == VehicleStatus.OnTrip)
            {
                trip.Vehicle.Status = VehicleStatus.Available;
            }

            if (trip.Driver.Status == DriverStatus.OnDuty)
            {
                trip.Driver.Status = DriverStatus.OffDuty;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Trip {trip.TripCode} cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get Vehicle Capacity
        [HttpGet]
        public async Task<JsonResult> GetVehicleCapacity(int vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle != null)
            {
                return Json(new
                {
                    capacity = vehicle.MaxLoadCapacity,
                    name = vehicle.Name,
                    odometer = vehicle.CurrentOdometer
                });
            }
            return Json(new { capacity = 0, name = "", odometer = 0 });
        }

        private async Task PopulateDropdowns()
        {
            // Only available vehicles (not retired, not on trip, not in shop)
            var availableVehicles = await _context.Vehicles
                .Where(v => v.Status == VehicleStatus.Available && !v.IsRetired)
                .OrderBy(v => v.Name)
                .Select(v => new { v.Id, Display = v.Name + " (" + v.LicensePlate + ") - " + v.MaxLoadCapacity + "kg" })
                .ToListAsync();

            ViewData["VehicleId"] = new SelectList(availableVehicles, "Id", "Display");

            // Only drivers with valid licenses (not suspended, not expired)
            var availableDrivers = await _context.Drivers
                .Where(d => d.Status != DriverStatus.Suspended && d.LicenseExpiry > DateTime.Now)
                .OrderBy(d => d.Name)
                .Select(d => new { d.Id, Display = d.Name + " (" + d.LicenseCategory + ")" })
                .ToListAsync();

            ViewData["DriverId"] = new SelectList(availableDrivers, "Id", "Display");
        }

        private string GenerateTripCode()
        {
            return $"TRP-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }
    }
}
