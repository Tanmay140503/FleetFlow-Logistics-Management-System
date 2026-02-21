namespace FleetFlow.ViewModels
{
    public class DashboardViewModel
    {
        public int ActiveFleet { get; set; }
        public int MaintenanceAlerts { get; set; }
        public decimal UtilizationRate { get; set; }
        public int PendingCargo { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public int CompletedTripsToday { get; set; }
        public decimal TodayRevenue { get; set; }
    }
}
