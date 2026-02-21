using System.ComponentModel.DataAnnotations;

namespace FleetFlow.Models
{
    public class FuelLog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a vehicle")]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }  // ✅ Made nullable

        public int? TripId { get; set; }
        public Trip? Trip { get; set; }  // ✅ Made nullable

        [Required(ErrorMessage = "Liters is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Liters must be greater than 0")]
        public decimal Liters { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0")]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Odometer reading is required")]
        [Range(0, double.MaxValue)]
        public decimal OdometerReading { get; set; }

        [StringLength(100)]
        public string? Station { get; set; }  // ✅ Made nullable

        public decimal PricePerLiter => Liters > 0 ? Cost / Liters : 0;
    }
}