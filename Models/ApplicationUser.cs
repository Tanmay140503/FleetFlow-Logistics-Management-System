using Microsoft.AspNetCore.Identity;

namespace FleetFlow.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public UserRole Role { get; set; }

    }
    public enum UserRole
    {
        FleetManager,
        Dispatcher,
        SafetyOfficer,
        FinancialAnalyst
    }
}
