using FleetFlow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace FleetFlow.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<FuelLog> FuelLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Vehicle - Unique License Plate
            builder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            // Trip relationships
            builder.Entity<Trip>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany(d => d.Trips)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Maintenance relationships
            builder.Entity<MaintenanceLog>()
                .HasOne(m => m.Vehicle)
                .WithMany(v => v.MaintenanceLogs)
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Fuel relationships
            builder.Entity<FuelLog>()
                .HasOne(f => f.Vehicle)
                .WithMany(v => v.FuelLogs)
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FuelLog>()
                .HasOne(f => f.Trip)
                .WithMany()
                .HasForeignKey(f => f.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            // Decimal precision
            builder.Entity<Vehicle>()
                .Property(v => v.MaxLoadCapacity)
                .HasPrecision(18, 2);

            builder.Entity<Vehicle>()
                .Property(v => v.CurrentOdometer)
                .HasPrecision(18, 2);

            builder.Entity<Trip>()
                .Property(t => t.CargoWeight)
                .HasPrecision(18, 2);

            builder.Entity<Trip>()
                .Property(t => t.Revenue)
                .HasPrecision(18, 2);

            builder.Entity<FuelLog>()
                .Property(f => f.Cost)
                .HasPrecision(18, 2);

            builder.Entity<FuelLog>()
                .Property(f => f.Liters)
                .HasPrecision(18, 2);

            builder.Entity<MaintenanceLog>()
                .Property(m => m.Cost)
                .HasPrecision(18, 2);

            builder.Entity<Driver>()
                .Property(d => d.SafetyScore)
                .HasPrecision(5, 2);
        }

    }
}

