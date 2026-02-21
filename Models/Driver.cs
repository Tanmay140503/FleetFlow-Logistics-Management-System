using System.ComponentModel.DataAnnotations;

namespace FleetFlow.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Driver name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "License number is required")]
        [StringLength(50)]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "License expiry date is required")]
        [DataType(DataType.Date)]
        public DateTime LicenseExpiry { get; set; }

        [Required(ErrorMessage = "License category is required")]
        [StringLength(50)]
        public string LicenseCategory { get; set; }

        public DriverStatus Status { get; set; } = DriverStatus.OffDuty;

        [Range(0, 100)]
        public decimal SafetyScore { get; set; } = 100;

        public int TotalTripsCompleted { get; set; } = 0;

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Now;

        // ✅ Navigation Property - Made nullable
        public ICollection<Trip>? Trips { get; set; }
    }

    public enum DriverStatus
    {
        OnDuty,
        OffDuty,
        Suspended
    }
}