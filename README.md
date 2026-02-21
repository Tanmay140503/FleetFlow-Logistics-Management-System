# FleetFlow-Logistics-Management-System


🎯 Project Overview

FleetFlow is a comprehensive **Fleet & Logistics Management System** built with **ASP.NET Core MVC** that transforms traditional pen-and-paper fleet operations into an intelligent, automated, and data-driven platform. Designed for logistics companies, delivery services, and fleet operators who need real-time visibility, compliance tracking, and operational efficiency.

### 🌟 Why FleetFlow?

| Problem | FleetFlow Solution |
|---------|-------------------|
| 📋 Manual logbooks prone to errors | ✅ Automated digital tracking with validation |
| 🚫 No real-time fleet visibility | ✅ Live dashboard with KPIs and status updates |
| 💸 Hidden costs and fuel wastage | ✅ Detailed expense tracking and ROI analysis |
| ⚠️ Missed maintenance schedules | ✅ Auto-status updates and preventive alerts |
| 📊 No performance insights | ✅ Advanced analytics and export capabilities |

---

## 🎨 Key Features

### 1️⃣ **Command Center Dashboard**
![Dashboard](https://img.shields.io/badge/Real--Time-Insights-success)

- **Live KPIs**: Active fleet, maintenance alerts, utilization rate, pending cargo
- **At-a-glance metrics**: Today's revenue, completed trips, driver statistics
- **Smart filters**: Filter by vehicle type, status, and region
- **Color-coded status pills**: Instant visual feedback on fleet health

### 2️⃣ **Vehicle Registry & Asset Management**
![CRUD Operations](https://img.shields.io/badge/Full-CRUD-blue)

- **Complete vehicle lifecycle** tracking (Creation → Active Use → Retirement)
- **Unique license plate** validation to prevent duplicates
- **Capacity management**: Max load tracking with trip validation
- **Auto-status updates**: Seamless transitions between Available/OnTrip/InShop
- **Detailed history**: View all trips, maintenance, and fuel logs per vehicle

### 3️⃣ **Intelligent Trip Dispatcher**
![Smart Validation](https://img.shields.io/badge/Smart-Validation-orange)

**Trip Lifecycle**: Draft → Dispatched → Completed → Cancelled

**Built-in Business Rules**:
- ✅ Cargo weight cannot exceed vehicle capacity
- ✅ Driver license must be valid and match vehicle type
- ✅ Auto-odometer tracking (start/end readings)
- ✅ Real-time vehicle/driver status synchronization
- ✅ Revenue tracking per trip

**Workflow Example**:
```
1. Create Trip (Draft) → Select Vehicle & Driver
2. Validate → Weight ≤ Capacity? License Valid? Type Match?
3. Dispatch → Vehicle: OnTrip, Driver: OnDuty
4. Complete → Record end odometer, revenue, notes
5. Auto-update → Vehicle & Driver back to Available/OffDuty
```

### 4️⃣ **Maintenance & Service Logs**
![Auto-Status](https://img.shields.io/badge/Auto--Status-Management-red)

- **Preventive tracking**: Schedule oil changes, tire rotations, inspections
- **Smart status logic**: Adding maintenance → Vehicle auto-marked "InShop"
- **Completion workflow**: Mark complete → Check for other pending work → Release vehicle
- **Cost tracking**: Per-service and total vehicle maintenance spend
- **Workshop management**: Record service provider details

### 5️⃣ **Driver Performance & Safety Profiles**
![Compliance](https://img.shields.io/badge/Safety-Compliance-yellow)

**Driver Features**:
- 📜 License expiry tracking with 30-day warnings
- 🏆 Safety score (0-100) with automatic updates
- 📊 Trip completion rate calculation
- 🚫 Auto-blocking expired licenses from trip assignment
- 📞 Contact management and status control (OnDuty/OffDuty/Suspended)

**Validation Rules**:
```csharp
✓ License Category matches Vehicle Type (Truck/Van/Bike)
✓ License Expiry > Today
✓ Driver not Suspended
✓ Driver not already assigned to active trip
```

### 6️⃣ **Fuel & Expense Logging**
![Financial Tracking](https://img.shields.io/badge/Financial-Tracking-green)

- **Detailed records**: Liters, cost, station, odometer reading
- **Auto-calculation**: Price per liter (Cost ÷ Liters)
- **Trip linkage**: Associate fuel logs with specific deliveries
- **Filtering**: By vehicle, date range
- **Summaries**: Total fuel consumed, average price/liter

### 7️⃣ **Operational Analytics & Reports**
![Data-Driven](https://img.shields.io/badge/Data--Driven-Decisions-purple)

**Vehicle Performance Report**:
- Total trips, distance traveled
- Fuel efficiency (km/L)
- Maintenance + Fuel costs
- Revenue and Net Profit
- **ROI Calculation**: `(Revenue - Costs) / Acquisition Cost × 100`

**Driver Performance Report**:
- Completion rate (Completed Trips / Total Trips)
- Safety score ranking
- Total distance driven
- Trip statistics

**Monthly Reports**:
- Aggregated trips, fuel, maintenance costs
- Revenue breakdown
- Export to Excel/PDF

### 8️⃣ **User Management & Role-Based Access**
![Security](https://img.shields.io/badge/Role--Based-Security-critical)

**4 User Roles**:

| Role | Permissions |
|------|------------|
| **Fleet Manager** | Full system access, user management, all CRUD operations |
| **Dispatcher** | Create/manage trips, view vehicles/drivers, limited reports |
| **Safety Officer** | Driver management, license compliance, safety scores |
| **Financial Analyst** | Read-only access, all reports, export capabilities |

**Features**:
- Secure authentication with ASP.NET Identity
- Password policies (6+ chars, uppercase, lowercase, digit)
- Account lockout after 5 failed attempts
- Fleet Managers can register new users

---

## 🏗️ Technical Architecture

### **Technology Stack**

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 8.0 MVC |
| **Frontend** | Razor Views + Bootstrap 5.3 |
| **Database** | SQL Server with Entity Framework Core |
| **Authentication** | ASP.NET Core Identity |
| **Icons** | Bootstrap Icons 1.11 |
| **Excel Export** | ClosedXML |

### **Project Structure**

```
FleetFlow/
├── 📁 Controllers/           # MVC Controllers
│   ├── AccountController.cs       # Auth & User Management
│   ├── DashboardController.cs     # Main Dashboard
│   ├── VehiclesController.cs      # Vehicle CRUD
│   ├── TripsController.cs         # Trip Management
│   ├── DriversController.cs       # Driver Management
│   ├── MaintenanceController.cs   # Maintenance Logs
│   ├── FuelLogsController.cs      # Fuel Tracking
│   └── ReportsController.cs       # Analytics & Export
│
├── 📁 Models/                # Domain Models
│   ├── ApplicationUser.cs         # User with Roles
│   ├── Vehicle.cs                 # Fleet Assets
│   ├── Driver.cs                  # Driver Profiles
│   ├── Trip.cs                    # Delivery Trips
│   ├── MaintenanceLog.cs          # Service Records
│   └── FuelLog.cs                 # Fuel Expenses
│
├── 📁 ViewModels/            # Data Transfer Objects
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── DashboardViewModel.cs
│   └── ReportViewModel.cs
│
├── 📁 Views/                 # Razor Views
│   ├── Account/                   # Login/Register
│   ├── Dashboard/                 # Main Dashboard
│   ├── Vehicles/                  # Vehicle CRUD Views
│   ├── Trips/                     # Trip Management
│   ├── Drivers/                   # Driver Management
│   ├── Maintenance/               # Maintenance Logs
│   ├── FuelLogs/                  # Fuel Tracking
│   ├── Reports/                   # Analytics
│   └── Shared/                    # Layout & Partials
│
├── 📁 Data/                  # Database Layer
│   ├── ApplicationDbContext.cs    # EF Core Context
│   └── SeedData.cs                # Initial Data
│
└── 📁 wwwroot/               # Static Files
    ├── css/
    ├── js/
    └── lib/

## 🚀 Installation & Setup

### **Prerequisites**

```bash
✓ .NET 8.0 SDK or later
✓ SQL Server 2019+ (or SQL Server Express/LocalDB)
✓ Visual Studio 2022 / VS Code / Rider
✓ Git (for version control)
```

### **Step 1: Clone Repository**

```bash
git clone https://github.com/yourusername/FleetFlow.git
cd FleetFlow
```

### **Step 2: Configure Database**

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FleetFlowDB;Trusted_Connection=True;"
  }
}
```

**For SQL Server**:
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=FleetFlowDB;User Id=YOUR_USER;Password=YOUR_PASS;TrustServerCertificate=True;"
```

### **Step 3: Restore Packages**

```bash
dotnet restore
```

### **Step 4: Apply Migrations**

```bash
# Create database and tables
dotnet ef database update

# If migrations don't exist, create them:
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### **Step 5: Run Application**

```bash
dotnet run
```

**Or in Visual Studio**: Press `F5`



## 🔐 Default Credentials

### **Pre-seeded User Accounts**

| Email | Password | Role | Access Level |
|-------|----------|------|--------------|
| `admin@fleetflow.com` | `Admin@123` | Fleet Manager | Full Access |
| `dispatcher@fleetflow.com` | `Dispatcher@123` | Dispatcher | Trips & Assignments |
| `safety@fleetflow.com` | `Safety@123` | Safety Officer | Driver Compliance |
| `finance@fleetflow.com` | `Finance@123` | Financial Analyst | Reports Only |

### **Sample Data**

The system auto-seeds:
- ✅ 3 Vehicles (Truck, Van, Bike)
- ✅ 2 Drivers with valid licenses
- ✅ User roles and permissions

