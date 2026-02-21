namespace FleetFlow.ViewModels
{
    public class VehicleReportViewModel
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string LicensePlate { get; set; }
        public int TotalTrips { get; set; }
        public decimal TotalDistance { get; set; }
        public decimal TotalFuelCost { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal FuelEfficiency { get; set; }
        public decimal ROI { get; set; }
    }

    public class DriverPerformanceViewModel
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal SafetyScore { get; set; }
        public decimal TotalDistance { get; set; }
    }
}
