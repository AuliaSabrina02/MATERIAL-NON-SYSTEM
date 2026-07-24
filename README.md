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

A web-based application designed to streamline the management of non-system materials by integrating material requests, reservations, receiving, put away, picking, and warehouse monitoring into a single platform.

</div>

---

# 📋 Overview

Material Non System is a web-based application developed to improve the management of non-system materials in warehouse operations. The system centralizes the entire material workflow, from request creation to warehouse monitoring, helping reduce manual processes and improve operational efficiency.

The application supports multiple user roles and provides real-time visibility into warehouse activities through an integrated dashboard.

---

# ✨ Features

## 🔐 Authentication & Authorization

- Secure user authentication
- Role-based access control
- Session management

## 📦 Material Request

- Create new material requests
- View request history
- Search and filter requests
- Track request status

## 📅 Material Reservation

- Reserve materials before picking
- View reservation history
- Track reservation status

## 📥 Receiving

- Record incoming materials
- Verify received materials
- Update receiving status

## 📦 Put Away

- Assign storage locations
- Manage rack and bin locations

## 🚚 Picking

- Process material picking
- Confirm picking completion
- Track delivery status

## 📊 Dashboard Monitoring

- Real-time dashboard
- Warehouse activity monitoring
- Material statistics

## 💬 Discussion Room

- Internal communication
- Discussion history for each request

## 📆 Project Schedule

- Schedule warehouse activities
- Calendar view
- Deadline monitoring

## ⚠ Priority Request

- Manage urgent requests
- Priority request processing

## 🚨 Pallet Problem

- Report damaged pallets
- Track problem resolution

---

# 🛠 Tech Stack

This project leverages a modern technology stack to build a scalable, maintainable, and user-friendly web application.

| Technology | Badge |
|------------|-------|
| **Backend Framework** | ![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core_MVC-512BD4?style=flat-square&logo=dotnet&logoColor=white) |
| **Programming Language** | ![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white) |
| **Database** | ![Microsoft SQL Server](https://img.shields.io/badge/Microsoft_SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white) |
| **Frontend** | ![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=flat-square&logo=html5&logoColor=white) ![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=flat-square&logo=css3&logoColor=white) ![Bootstrap](https://img.shields.io/badge/Bootstrap-7952B3?style=flat-square&logo=bootstrap&logoColor=white) ![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=flat-square&logo=javascript&logoColor=black) ![jQuery](https://img.shields.io/badge/jQuery-0769AD?style=flat-square&logo=jquery&logoColor=white) |
| **IDE** | ![Visual Studio](https://img.shields.io/badge/Visual_Studio-5C2D91?style=flat-square&logo=visualstudio&logoColor=white) |
| **Version Control** | ![Git](https://img.shields.io/badge/Git-F05032?style=flat-square&logo=git&logoColor=white) ![GitHub](https://img.shields.io/badge/GitHub-181717?style=flat-square&logo=github&logoColor=white) |

---

# ⚙️ Installation

## Prerequisites

- Visual Studio 2022
- .NET SDK
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- Git

## Clone Repository

```bash
git clone https://github.com/AuliaSabrina02/MATERIAL-NON-SYSTEM.git
cd MATERIAL-NON-SYSTEM
```

## Configure Database

Update the SQL Server connection string in:

```text
appsettings.json
```

Configure the connection according to your local SQL Server instance.

## Restore and Run

```bash
dotnet restore
dotnet build
dotnet run
```

Or simply open the solution file (`SEMB_ERP.sln`) in **Visual Studio** and press **F5**.

---

# 📂 Project Structure

```text
SEMB_ERP/
├── Controllers/
├── Models/
├── Views/
├── Services/
├── Data/
├── Helpers/
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── images/
│   └── uploads/
├── Program.cs
├── appsettings.json
└── SEMB_ERP.sln
```

---

# 👤 Contributors

| Name | Role |
|------|------|
| Aulia Sabrina | Full Stack Developer |

---

# 📄 License

This project was developed for educational purposes as part of a Final Project.
