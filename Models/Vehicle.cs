using System.ComponentModel.DataAnnotations;

namespace FleetFlow.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vehicle name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "License plate is required")]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [Required]
        public VehicleType Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
        public decimal MaxLoadCapacity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CurrentOdometer { get; set; }

        public VehicleStatus Status { get; set; } = VehicleStatus.Available;

        [StringLength(50)]
        public string? Region { get; set; }

        public bool IsRetired { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public decimal? AcquisitionCost { get; set; }

        // ✅ Navigation Properties - Made nullable
        public ICollection<Trip>? Trips { get; set; }
        public ICollection<MaintenanceLog>? MaintenanceLogs { get; set; }
        public ICollection<FuelLog>? FuelLogs { get; set; }
    }

    public enum VehicleType
    {
        Truck,
        Van,
        Bike
    }

    public enum VehicleStatus
    {
        Available,
        OnTrip,
        InShop,
        OutOfService
    }
}