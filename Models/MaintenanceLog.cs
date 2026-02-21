using System.ComponentModel.DataAnnotations;

namespace FleetFlow.Models
{
    public class MaintenanceLog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a vehicle")]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }  // ✅ Made nullable

        [Required(ErrorMessage = "Service type is required")]
        [StringLength(100)]
        public string ServiceType { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }  // ✅ Made nullable

        [Required(ErrorMessage = "Cost is required")]
        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "Service date is required")]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; }

        [Required(ErrorMessage = "Odometer reading is required")]
        [Range(0, double.MaxValue)]
        public decimal OdometerAtService { get; set; }

        [StringLength(100)]
        public string? PerformedBy { get; set; }  // ✅ Made nullable

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }
    }
}