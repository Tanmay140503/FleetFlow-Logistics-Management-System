using System.ComponentModel.DataAnnotations;

namespace FleetFlow.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? TripCode { get; set; }  // ✅ Made nullable - auto-generated

        [Required(ErrorMessage = "Please select a vehicle")]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }  // ✅ Made nullable

        [Required(ErrorMessage = "Please select a driver")]
        public int DriverId { get; set; }
        public Driver? Driver { get; set; }  // ✅ Made nullable

        [Required(ErrorMessage = "Origin address is required")]
        [StringLength(200)]
        public string OriginAddress { get; set; }

        [Required(ErrorMessage = "Destination address is required")]
        [StringLength(200)]
        public string DestinationAddress { get; set; }

        [Required(ErrorMessage = "Cargo weight is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cargo weight must be greater than 0")]
        public decimal CargoWeight { get; set; }

        [StringLength(500)]
        public string? CargoDescription { get; set; }  // ✅ Nullable

        public TripStatus Status { get; set; } = TripStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DispatchedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public decimal StartOdometer { get; set; }
        public decimal? EndOdometer { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Revenue { get; set; }

        public string? Notes { get; set; }  // ✅ Made nullable

        // Computed property
        public decimal? DistanceTraveled => EndOdometer.HasValue ? EndOdometer.Value - StartOdometer : null;
    }

    public enum TripStatus
    {
        Draft,
        Dispatched,
        Completed,
        Cancelled
    }
}