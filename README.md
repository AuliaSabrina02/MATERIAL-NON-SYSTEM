<div align="center">

# 📦 Material Non System


<p>
<img src="https://img.shields.io/badge/ASP.NET_Core_MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
<img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white">
<img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white">
<img src="https://img.shields.io/badge/Bootstrap-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white">
<img src="https://img.shields.io/badge/jQuery-0769AD?style=for-the-badge&logo=jquery&logoColor=white">
</p>

Sistem pengelolaan **Material Non System** berbasis web yang membantu proses material request, reservation, receiving, put away, picking, serta monitoring aktivitas warehouse secara terintegrasi.

</div>

---

# 📋 Overview

Material Non System adalah aplikasi berbasis web yang dikembangkan untuk membantu pengelolaan material non-system di lingkungan warehouse PT XYZ Manufacturing Batam. Sistem ini dirancang untuk mengurangi proses manual, meningkatkan efisiensi operasional, serta menyediakan monitoring status material secara real-time.

Aplikasi mendukung seluruh alur pengelolaan material mulai dari proses request, reservation, receiving, put away, picking, hingga monitoring melalui dashboard dalam satu platform yang terintegrasi.

---

# ✨ Features

## 🔐 Authentication & Authorization

- Login Authentication
- Role-based Access Control
- Session Management

## 📦 Material Request

- Create Material Request
- Request History
- Request Status Tracking
- Search & Filter Request

## 📅 Material Reservation

- Material Reservation
- Reservation Status
- Reservation History

## 📥 Receiving

- Material Receiving
- Material Verification
- Receiving Status

## 📦 Put Away

- Storage Location Assignment
- Rack/Bin Management

## 🚚 Picking

- Picking Process
- Picking Confirmation
- Material Delivery Tracking

## 📊 Dashboard Monitoring

- Material Statistics
- Warehouse Monitoring
- Real-time Dashboard

## 💬 Discussion Room

- Discussion per Request
- Internal Communication

## 📆 Project Schedule

- Schedule Management
- Calendar View
- Deadline Monitoring

## ⚠ Priority Request

- Priority Material Request
- Fast Request Processing

## 🚨 Pallet Problem

- Report Damaged Pallet
- Problem Monitoring

---

# 🛠 Tech Stack

| Layer | Technology |
|--------|------------|
| Framework | ASP.NET Core MVC |
| Language | C# |
| Database | Microsoft SQL Server |
| Frontend | HTML5, CSS3, Bootstrap |
| Client Script | JavaScript, jQuery |
| IDE | Visual Studio 2022 |
| Version Control | Git & GitHub |

---

# ⚙ Installation

## Prerequisites

- Visual Studio 2022
- .NET SDK
- SQL Server
- SQL Server Management Studio (SSMS)

## Clone Repository

```bash
git clone https://github.com/AuliaSabrina02/MATERIAL-NON-SYSTEM.git
```

## Configure Database

Update the SQL Server connection string inside:

```
appsettings.json
```

## Run Application

```bash
dotnet restore
dotnet build
dotnet run
```

atau jalankan melalui **Visual Studio** dengan menekan **F5**.

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

This project was developed for educational purposes as a Final Project.
