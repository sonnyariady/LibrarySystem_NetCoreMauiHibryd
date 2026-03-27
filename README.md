# 📚 Library Online System

## 🇮🇩 Deskripsi

Sistem perpustakaan online berbasis:

* .NET 9
* ASP.NET Core Web API
* MudBlazor 7.4.0
* Blazor Web (Server)
* .NET MAUI Hybrid (Windows & Android)

Mendukung:

* Generic CRUD API
* Multi search (keyword + filter)
* UI reusable (Web & MAUI)

---

## 🇬🇧 Description

An online library management system built with:

* .NET 9
* ASP.NET Core Web API
* MudBlazor 7.4.0
* Blazor Web (Server)
* .NET MAUI Hybrid (Windows & Android)

Supports:

* Generic CRUD API
* Multi-field search
* Shared UI (Web & MAUI)

---

## 📦 Project Structure

```
src/
 ├── Library.Api     → Backend API
 ├── Library.UI      → Shared UI (Pages & Layout)
 ├── Library.Web     → Web Host
 ├── Library.App     → MAUI Hybrid App
```

---

## 🚀 Run Application

### API

```bash
dotnet run --project src/Library.Api
```

Swagger:

```
https://localhost:65090/swagger
```

---

### Web UI

```bash
dotnet run --project src/Library.Web
```

```
https://localhost:8080
```

---

### MAUI

Run project `Library.App` via Visual Studio.

---

## 🧱 Features

### Backend

* Generic CRUD Controller
* EF Core
* Search & filtering

### Frontend

* MudBlazor UI
* Responsive layout
* Shared components

---

## 🗄 Database Migration

```bash
dotnet ef migrations add InitialCreate --project src/Library.Api
dotnet ef database update --project src/Library.Api
```

---

## ⚠️ Notes

* API root `/` mungkin tidak ada (gunakan `/swagger`)
* UI root `/` hanya tersedia di `Library.Web`
* Pastikan `Library.UI` sudah direference oleh `Library.Web`

---

## 🔮 Future Improvements

* JWT Authentication
* Role-based access
* Borrow/Return workflow
* Dashboard analytics
* Docker deployment

---

## 👨‍💻 Author

Library System – .NET 9 + MudBlazor + MAUI Hybrid
