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

Material Non System is a web-based warehouse management application developed to support the management of non-system materials at PT XYZ Manufacturing Batam.

The system is designed to improve warehouse operational efficiency by reducing manual processes, centralizing material information, and providing better visibility of material activities.

The application manages the complete material workflow, starting from material requests, reservation, receiving, put away, picking, scheduling, and monitoring through an integrated dashboard.

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

# ⚙️ Installation & Setup

## Prerequisites

Before running this project, make sure the following tools are installed:

- Visual Studio 2022
- .NET SDK
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- Git

---

## Clone Repository

```bash
git clone https://github.com/AuliaSabrina02/MATERIAL-NON-SYSTEM.git

cd MATERIAL-NON-SYSTEM
```

---

## Configure Database

Update the database connection string in:

```text
appsettings.json
```

Configure the connection according to your SQL Server environment.

---

## Restore Database

Restore the required SQL Server database before running the application.

---

## Build and Run

Using command line:

```bash
dotnet restore

dotnet build

dotnet run
```

Or open the solution file:

```text
SEMB_ERP.sln
```

using Visual Studio and press:

```
F5 (Start Debugging)
```

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
