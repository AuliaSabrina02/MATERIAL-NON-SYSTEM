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

A web-based Material Non System management application designed to streamline material requests, reservations, receiving, put away, picking, and warehouse monitoring within a single integrated platform.

</div>

---

# 📋 Overview

Material Non System is a web-based application developed to support the management of non-system materials in warehouse operations at PT XYZ Manufacturing Batam.

The system aims to improve operational efficiency by reducing manual processes, centralizing material management, and providing real-time monitoring throughout the entire material lifecycle.

It supports end-to-end warehouse operations, including material requests, reservations, receiving, put away, picking, scheduling, and monitoring through an integrated dashboard.

---

# ✨ Features

## 🔐 Authentication & Authorization

- Secure user authentication
- Role-based access control
- Session management

## 📦 Material Request

- Create material requests
- View request history
- Track request status
- Search and filter requests

## 📅 Material Reservation

- Reserve materials before picking
- Track reservation status
- View reservation history

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
- Track material delivery

## 📊 Dashboard Monitoring

- Real-time warehouse dashboard
- Material statistics and reports
- Operational monitoring

## 💬 Discussion Room

- Internal communication for requests
- Discussion history and collaboration

## 📆 Project Schedule

- Schedule warehouse activities
- Calendar-based planning
- Deadline monitoring

## ⚠ Priority Request

- Manage high-priority requests
- Accelerate request processing

## 🚨 Pallet Problem

- Report damaged pallets
- Monitor issue resolution

---

# 🛠 Tech Stack

| Layer | Technology |
|--------|------------|
| Framework | ASP.NET Core MVC |
| Programming Language | C# |
| Database | Microsoft SQL Server |
| Frontend | HTML5, CSS3, Bootstrap |
| Client-side Scripting | JavaScript, jQuery |
| IDE | Visual Studio 2022 |
| Version Control | Git & GitHub |

---

# ⚙ Installation & Setup

## Prerequisites

- Visual Studio 2022
- .NET SDK
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)

## Clone the Repository

```bash
git clone https://github.com/AuliaSabrina02/MATERIAL-NON-SYSTEM.git
```

## Configure the Database

Update the SQL Server connection string in:

```text
appsettings.json
```

## Build and Run

```bash
dotnet restore
dotnet build
dotnet run
```

Or simply open the solution in **Visual Studio** and press **F5** to start the application.

---

# 📂 Project Structure

```text
SEMB_ERP/
├── Controllers/
├── Models/
├── Views/
├── wwwroot/
├── Services/
├── Data/
├── Helpers/
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
