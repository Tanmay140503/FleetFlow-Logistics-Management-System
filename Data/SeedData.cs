using FleetFlow.Models;
using Microsoft.AspNetCore.Identity;

namespace FleetFlow.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
     IServiceProvider serviceProvider,
     UserManager<ApplicationUser> userManager,
     RoleManager<IdentityRole> roleManager)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create Roles
            string[] roleNames = { "FleetManager", "Dispatcher", "SafetyOfficer", "FinancialAnalyst" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create Users
            await CreateUser(userManager, "admin@fleetflow.com", "Admin@123", "Admin User", UserRole.FleetManager, "FleetManager");
            await CreateUser(userManager, "dispatcher@fleetflow.com", "Dispatcher@123", "John Dispatcher", UserRole.Dispatcher, "Dispatcher");
            await CreateUser(userManager, "safety@fleetflow.com", "Safety@123", "Sarah Safety", UserRole.SafetyOfficer, "SafetyOfficer");
            await CreateUser(userManager, "finance@fleetflow.com", "Finance@123", "Mike Finance", UserRole.FinancialAnalyst, "FinancialAnalyst");

            //// Create Default Admin User
            //var adminEmail = "admin@fleetflow.com";
            //var adminUser = await userManager.FindByEmailAsync(adminEmail);

            //if (adminUser == null)
            //{
            //    var newAdmin = new ApplicationUser
            //    {
            //        UserName = adminEmail,
            //        Email = adminEmail,
            //        FullName = "System Administrator",
            //        Role = UserRole.FleetManager,
            //        EmailConfirmed = true
            //    };

            //    var result = await userManager.CreateAsync(newAdmin, "Admin@123");

            //    if (result.Succeeded)
            //    {
            //        await userManager.AddToRoleAsync(newAdmin, "FleetManager");
            //    }
            //}

            // Seed Sample Vehicles
            if (!context.Vehicles.Any())
            {
                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        Name = "Toyota Hiace 2023",
                        LicensePlate = "ABC-1234",
                        Type = VehicleType.Van,
                        MaxLoadCapacity = 1500,
                        CurrentOdometer = 5000,
                        Status = VehicleStatus.Available,
                        Region = "North",
                        AcquisitionCost = 45000
                    },
                    new Vehicle
                    {
                        Name = "Isuzu D-Max 2022",
                        LicensePlate = "XYZ-5678",
                        Type = VehicleType.Truck,
                        MaxLoadCapacity = 3000,
                        CurrentOdometer = 12000,
                        Status = VehicleStatus.Available,
                        Region = "South",
                        AcquisitionCost = 65000
                    },
                    new Vehicle
                    {
                        Name = "Honda CBR 2024",
                        LicensePlate = "MNO-9012",
                        Type = VehicleType.Bike,
                        MaxLoadCapacity = 50,
                        CurrentOdometer = 2000,
                        Status = VehicleStatus.Available,
                        Region = "East",
                        AcquisitionCost = 5000
                    }
                };

                context.Vehicles.AddRange(vehicles);
                await context.SaveChangesAsync();
            }

            // Seed Sample Drivers
            if (!context.Drivers.Any())
            {
                var drivers = new List<Driver>
                {
                    new Driver
                    {
                        Name = "John Doe",
                        Email = "john@fleetflow.com",
                        LicenseNumber = "DL-123456",
                        LicenseExpiry = DateTime.Now.AddYears(2),
                        LicenseCategory = "Van, Truck",
                        Status = DriverStatus.OffDuty,
                        SafetyScore = 95,
                        PhoneNumber = "1234567890"
                    },
                    new Driver
                    {
                        Name = "Jane Smith",
                        Email = "jane@fleetflow.com",
                        LicenseNumber = "DL-789012",
                        LicenseExpiry = DateTime.Now.AddYears(1),
                        LicenseCategory = "Van",
                        Status = DriverStatus.OffDuty,
                        SafetyScore = 98,
                        PhoneNumber = "0987654321"
                    }
                };

                context.Drivers.AddRange(drivers);
                await context.SaveChangesAsync();
            }
        }
        private static async Task CreateUser(
    UserManager<ApplicationUser> userManager,
    string email,
    string password,
    string fullName,
    UserRole role,
    string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, roleName);
                }
            }
        }
    }

}

