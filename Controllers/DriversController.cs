using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetFlow.Data;
using FleetFlow.Models;

namespace FleetFlow.Controllers
{
    [Authorize(Roles = "FleetManager,SafetyOfficer")] // Both can manage drivers

    public class DriversController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DriversController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Drivers
        public async Task<IActionResult> Index()
        {
            var drivers = await _context.Drivers
                .Include(d => d.Trips)
                .OrderBy(d => d.Name)
                .ToListAsync();

            // Check for expiring licenses (within 30 days)
            var expiringLicenses = drivers.Where(d =>
                d.LicenseExpiry <= DateTime.Now.AddDays(30) &&
                d.LicenseExpiry > DateTime.Now).ToList();

            ViewBag.ExpiringLicenses = expiringLicenses;

            return View(drivers);
        }

        // GET: Drivers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _context.Drivers
                .Include(d => d.Trips.OrderByDescending(t => t.CreatedAt).Take(20))
                .ThenInclude(t => t.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (driver == null) return NotFound();

            // Calculate performance metrics
            var completedTrips = driver.Trips.Count(t => t.Status == TripStatus.Completed);
            var totalTrips = driver.Trips.Count();
            var completionRate = totalTrips > 0 ? (decimal)completedTrips / totalTrips * 100 : 0;
            var totalDistance = driver.Trips
                .Where(t => t.EndOdometer.HasValue)
                .Sum(t => t.EndOdometer.Value - t.StartOdometer);

            ViewBag.CompletionRate = completionRate;
            ViewBag.TotalDistance = totalDistance;

            return View(driver);
        }

        // GET: Drivers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drivers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,LicenseNumber,LicenseExpiry,LicenseCategory,PhoneNumber")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate license number
                if (await _context.Drivers.AnyAsync(d => d.LicenseNumber == driver.LicenseNumber))
                {
                    ModelState.AddModelError("LicenseNumber", "This license number already exists.");
                    return View(driver);
                }

                // Check for duplicate email
                if (await _context.Drivers.AnyAsync(d => d.Email == driver.Email))
                {
                    ModelState.AddModelError("Email", "This email already exists.");
                    return View(driver);
                }

                driver.Status = DriverStatus.OffDuty;
                driver.SafetyScore = 100;
                driver.TotalTripsCompleted = 0;
                driver.HireDate = DateTime.Now;

                _context.Add(driver);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Driver added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Drivers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return NotFound();

            return View(driver);
        }

        // POST: Drivers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,LicenseNumber,LicenseExpiry,LicenseCategory,Status,SafetyScore,TotalTripsCompleted,PhoneNumber,HireDate")] Driver driver)
        {
            if (id != driver.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driver);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Driver updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // POST: Drivers/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, DriverStatus newStatus)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return NotFound();

            // Cannot change status if driver is on a trip
            var hasActiveTrip = await _context.Trips
                .AnyAsync(t => t.DriverId == id && t.Status == TripStatus.Dispatched);

            if (hasActiveTrip)
            {
                TempData["Error"] = "Cannot change status while driver has an active trip.";
                return RedirectToAction(nameof(Index));
            }

            driver.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Driver status changed to {newStatus}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Drivers/Suspend/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id, string reason)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return NotFound();

            driver.Status = DriverStatus.Suspended;
            // You could add a suspension reason field to the model

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Driver {driver.Name} has been suspended.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Drivers/Reactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return NotFound();

            driver.Status = DriverStatus.OffDuty;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Driver {driver.Name} has been reactivated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Drivers/UpdateSafetyScore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSafetyScore(int id, decimal newScore, string reason)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return NotFound();

            if (newScore < 0 || newScore > 100)
            {
                TempData["Error"] = "Safety score must be between 0 and 100.";
                return RedirectToAction(nameof(Details), new { id });
            }

            driver.SafetyScore = newScore;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Safety score updated to {newScore}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
}
