<div align="center">

# 📦 Material Non System

### Web-Based Material Management System

<p>
<img src="https://img.shields.io/badge/ASP.NET_Core_MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
<img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white">
<img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white">
<img src="https://img.shields.io/badge/Bootstrap-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white">
<img src="https://img.shields.io/badge/jQuery-0769AD?style=for-the-badge&logo=jquery&logoColor=white">
</p>

A web-based application designed to manage non-system materials by integrating material requests, reservations, receiving, put away, picking, and warehouse monitoring into a centralized platform.

</div>

---

# 📋 Overview

Material Non System is a web-based warehouse management application developed to support non-system material management in a manufacturing environment.

The system is designed to improve warehouse operational efficiency by reducing manual processes, organizing material information, and providing better visibility of material activities.

The application manages the material workflow, including material requests, reservations, receiving, put away, picking, scheduling, and monitoring through an integrated dashboard.

---

# ✨ Features

## 🔐 Authentication & Authorization

- User authentication
- Role-based access control
- Session management
- User access management

---

## 📦 Material Request

- Create new material requests
- View request history
- Track request status
- Search and filter request data

---

## 📅 Material Reservation

- Reserve materials before picking
- Manage reservation data
- Track reservation status
- View reservation history

---

## 📥 Receiving

- Record incoming materials
- Verify received materials
- Update receiving status

---

## 📦 Put Away

- Assign storage locations
- Manage rack and bin information
- Update material storage status

---

## 🚚 Picking

- Process material picking activities
- Confirm picking process
- Track material delivery status

---

## 📊 Dashboard Monitoring

- Real-time warehouse monitoring
- Material activity statistics
- Operational status visualization

---

## 💬 Discussion Room

- Internal communication between users
- Discussion history management
- Request-related discussions

---

## 📆 Project Schedule

- Manage project schedules
- Calendar-based activity planning
- Monitor project deadlines

---

## ⚠ Priority Request

- Manage urgent material requests
- Prioritize important requests
- Improve request processing efficiency

---

## 🚨 Pallet Problem

- Report pallet issues
- Monitor problem resolution
- Track reported problems

---

# 👥 User Roles

| Role | Description |
|------|-------------|
| Admin | Manage system configuration and user access |
| Requestor | Create and monitor material requests |
| Receiver | Handle receiving activities |
| Plant Receiver | Receive and confirm delivered materials |

---

# 🛠 Tech Stack

This project leverages a modern technology stack to build a scalable, maintainable, and user-friendly web application.

| Technology | Badge |
|------------|-------|
| **Backend Framework** | ![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core_MVC-512BD4?style=flat-square&logo=dotnet&logoColor=white) |
| **Programming Language** | ![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white) |
| **Database** | ![Microsoft SQL Server](https://img.shields.io/badge/Microsoft_SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white) |
| **Frontend** | ![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=flat-square&logo=html5&logoColor=white) ![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=flat-square&logo=css3&logoColor=white) ![Bootstrap](https://img.shields.io/badge/Bootstrap-7952B3?style=flat-square&logo=bootstrap&logoColor=white) |
| **Client-side Script** | ![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=flat-square&logo=javascript&logoColor=black) ![jQuery](https://img.shields.io/badge/jQuery-0769AD?style=flat-square&logo=jquery&logoColor=white) |
| **IDE** | ![Visual Studio](https://img.shields.io/badge/Visual_Studio-5C2D91?style=flat-square&logo=visualstudio&logoColor=white) |
| **Version Control** | ![Git](https://img.shields.io/badge/Git-F05032?style=flat-square&logo=git&logoColor=white) ![GitHub](https://img.shields.io/badge/GitHub-181717?style=flat-square&logo=github&logoColor=white) |

---

# ⚙️ Instalasi (Installation)

Follow these steps to get SEMB ERP up and running on your local machine.

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET SDK**: Version 6.0 or newer.
- **Visual Studio**: 2019 or newer (Community, Professional, or Enterprise edition). Alternatively, you can use Visual Studio Code with the C# extension.
- **SQL Server**: SQL Server LocalDB, SQL Server Express, or a full SQL Server instance.

---

## Step-by-Step Installation

### 1. Clone the Repository

Open your terminal or command prompt and clone the project:

```bash
git clone https://github.com/AuliaSabrina02/MATERIAL-NON-SYSTEM.git
```

### 2. Navigate to Project Directory

Change your directory to the main project folder:

```bash
cd MATERIAL-NON-SYSTEM/SEMB_ERP
```

### 3. Open in Visual Studio

Open the solution file `SEMB_ERP.sln` in Visual Studio.

### 4. Restore NuGet Packages

Visual Studio should automatically restore all necessary NuGet packages. If not, right-click on the solution in the Solution Explorer and select **"Restore NuGet Packages"**.

### 5. Database Configuration

- Open `appsettings.json` (and `appsettings.Development.json` for development environment settings) located in the `SEMB_ERP` project folder.
- Update the `ConnectionStrings:DefaultConnection` entry to point to your local SQL Server instance.

Example for SQL Server LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SembErpDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 6. Database Setup (Choose ONE option)

**Option A: Using Entity Framework Core Migrations (Recommended)**

1. Open the Package Manager Console in Visual Studio (Go to **Tools > NuGet Package Manager > Package Manager Console**).
2. Ensure `SEMB_ERP` is selected as the default project in the console dropdown.
3. Run the following command to apply any pending migrations and create the database schema:

```powershell
Update-Database
```

**Option B: Manual Database Setup**

If Entity Framework Core migrations are not configured or preferred, you will need to manually create the database and its tables. Review the `Models` directory and `DatabaseAccessLayer.cs` for schema details and implement your database creation script accordingly.

### 7. Build the Project

Build the entire solution to ensure all dependencies are resolved and the project compiles successfully. You can do this by going to **Build > Build Solution** in Visual Studio, or by pressing **Ctrl + Shift + B**.

### 8. Run the Application

- Press **F5** in Visual Studio to run the application with debugging.
- Alternatively, open a terminal in the `SEMB_ERP` project directory and run:

```bash
dotnet run
```

The application will typically launch in your default web browser at an address like `https://localhost:xxxx` (where `xxxx` is a dynamically assigned port number).

---

# 📂 Project Structure

The project follows a well-organized **ASP.NET Core MVC** architecture, promoting modularity, maintainability, and separation of concerns.

```text
MATERIAL-NON-SYSTEM/
├── .gitignore
├── README.md
├── SEMB_ERP.sln                         # Visual Studio solution file
│
└── SEMB_ERP/                            # Main ASP.NET Core MVC project
    │
    ├── Controllers/                     # Handle HTTP requests and application logic
    │   ├── AccountController.cs         # User authentication and account management
    │   ├── AuthController.cs            # Authorization and security handling
    │   └── ...                          # Other controllers
    │
    ├── Function/                        # Core business logic and utility functions
    │   ├── ApplicationDbContext.cs      # Database context configuration
    │   ├── Authentication.cs            # Authentication helper functions
    │   ├── DatabaseAccessLayer.cs       # Database access layer
    │   └── ...                          # Other utility functions
    │
    ├── Models/                          # Data structures and business objects
    │   ├── LoginModel.cs
    │   ├── MaterialReservationModel.cs
    │   ├── OrderListModel.cs
    │   └── ...                          # Other entity models
    │
    ├── Properties/                      # Project-level settings
    │
    ├── Service/                         # Business services and external integrations
    │   ├── ExcelServiceProvider.cs      # Excel import/export service
    │   ├── FileManagementService.cs     # File handling service
    │   └── ...                          # Other services
    │
    ├── Views/                           # Razor View templates
    │   ├── Home/
    │   ├── Shared/
    │   └── User/
    │       ├── DashboardAdmin.cshtml
    │       ├── OrderList.cshtml
    │       └── ...                      # Other user interface pages
    │
    ├── wwwroot/                         # Static files (CSS, JavaScript, images)
    │
    ├── appsettings.json                 # Application configuration settings
    ├── appsettings.Development.json     # Development configuration
    ├── appsettings.Production.json      # Production configuration
    │
    └── Program.cs                       # Application entry point and configuration
```

---

# 🧪 Testing

The application has been tested to ensure that implemented features work according to the defined requirements and user needs.

Testing includes:

- Functional testing
- User acceptance testing
- Feature validation

---

# 👤 Contributors

| Name | Role |
|------|------|
| Aulia Sabrina | Full Stack Developer |

---

# 📄 License

This project was developed for educational purposes as part of a Final Project.
