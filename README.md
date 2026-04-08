# 🏥 HCARS
### Healthcare Clinic Appointment & Resource System

---

## 📌 Project Overview

**HCARS** (Healthcare Clinic Appointment & Resource System) is a full-stack web-based clinic management system developed using **ASP.NET Core**.

The system replaces manual clinic operations with a complete digital solution for managing appointments, doctors, patients, medical records, prescriptions, and operational reporting — all in one platform.

The solution is built on a **multi-project architecture**:
- **Healthcare.API** — ASP.NET Core Web API (Backend & data layer)
- **Healthcare.MVC** — ASP.NET Core MVC Application (Main user-facing system)
- **Healthcare.Reporting** — Separate read-only reporting client (API-based)

All three projects are deployed independently on **Microsoft Azure**.

---

## 👥 Team Members

| Student ID | Name |
|-----------|------|
| 202304661 | Fatema Maitham |
| 202305590 | Maram Shubbar |
| 202300641 | Malak Almajed |
| 202302211 | Zainab Almahdi |
| 202302702 | Kawther Abdulla |

---

## ⚙️ Roles & Responsibilities

| Member | Role | Work Owned |
|--------|------|------------|
| **Fatema Maitham** | MVC Developer | Appointment booking pages, appointment lifecycle (Requested → Confirmed → Completed), patient record views, EF Core integration inside MVC |
| **Maram Shubbar** | MVC Developer | Doctor schedule management pages, role-based dashboards (Patient / Doctor / Receptionist), authentication-related MVC views, clinic workflow features |
| **Malak Almajed** | API & Backend Developer | RESTful Web API design and implementation, EF Core database connection, JWT authentication, public lookup endpoint, all API security |
| **Zainab Almahdi** | UI/UX Designer | Bootstrap layout and styling, responsiveness across devices, navigation improvements, jQuery/AJAX interactive features, overall user experience |
| **Kawther Abdulla** | Reporting Developer | Reporting application project, HttpClient API consumption, appointment/doctor/cancellation reports, read-only reporting enforcement |

---

## 📂 Detailed Project Structure

```
HCARS Solution/
│
├── Healthcare.API/                              // Backend Web API
│   │
│   ├── Controllers/
│   │   ├── AppointmentsController.cs            // CRUD endpoints for appointments, filter by doctor/patient/status
│   │   ├── DoctorsController.cs                 // Doctor management, schedules, specialization filter
│   │   ├── PatientsController.cs                // Patient registration, records, profile management
│   │   ├── MedicalRecordsController.cs          // Visit records linked to completed appointments
│   │   ├── PrescriptionsController.cs           // Prescriptions linked to appointments and patients
│   │   ├── AuthController.cs                    // Login, registration, JWT token generation
│   │   ├── PublicController.cs                  // Public lookup endpoints (no authentication required)
│   │   ├── NotificationsController.cs           // User notification retrieval and read status
│   │   └── ReportsController.cs                 // Read-only report endpoints consumed by Reporting App
│   │
│   ├── Models/                                  // EF Core entity classes (database tables)
│   │   ├── Appointment.cs                       // Appointment entity (status, date, doctor, patient)
│   │   ├── Doctor.cs                            // Doctor entity (name, specialization, schedule)
│   │   ├── Patient.cs                           // Patient entity (CPR, contact info, history)
│   │   ├── MedicalRecord.cs                     // Visit record (diagnosis, notes, treatment)
│   │   ├── Prescription.cs                      // Prescription (medication, dosage, linked to record)
│   │   ├── Notification.cs                      // Notification (message, read status, user)
│   │   ├── AppUser.cs                           // ASP.NET Identity user extension
│   │   └── DoctorSchedule.cs                    // Available working slots per doctor
│   │
│   ├── DTOs/                                    // Data Transfer Objects (API input/output shapes)
│   │   ├── AppointmentDto.cs                    // Appointment create/update/response DTO
│   │   ├── DoctorDto.cs                         // Doctor profile and schedule DTO
│   │   ├── PatientDto.cs                        // Patient registration and profile DTO
│   │   ├── MedicalRecordDto.cs                  // Record creation and response DTO
│   │   ├── PrescriptionDto.cs                   // Prescription DTO
│   │   └── AuthDto.cs                           // Login request and JWT token response DTO
│   │
│   ├── Data/
│   │   ├── AppDbContext.cs                      // EF Core DbContext, table configs, relationships
│   │   └── Migrations/                          // Auto-generated EF Core database migrations
│   │
│   ├── Services/                                // Business logic (separated from controllers)
│   │   ├── AppointmentService.cs                // Booking rules, slot checking, status transitions
│   │   ├── AuthService.cs                       // JWT generation, password hashing, user validation
│   │   ├── NotificationService.cs               // Create and dispatch in-app notifications
│   │   └── ReportService.cs                     // Data aggregation for reporting endpoints
│   │
│   ├── Hubs/
│   │   └── AppointmentHub.cs                    // SignalR hub — broadcasts real-time status updates
│   │
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs        // Global error handling, consistent error responses
│   │
│   ├── appsettings.json                         // Connection string, JWT secret, Azure config
│   └── Program.cs                               // API startup: middleware, JWT, EF Core, SignalR, CORS
│
│
├── Healthcare.MVC/                              // Main Web Application
│   │
│   ├── Controllers/
│   │   ├── AppointmentController.cs             // Book, view, cancel, update appointment pages
│   │   ├── PatientController.cs                 // Patient dashboard, visit history, profile
│   │   ├── DoctorController.cs                  // Doctor profile, schedule management views 
│   │   ├── DashboardController.cs               // Role-based dashboard routing
│   │   ├── AccountController.cs                 // Login, logout, registration MVC views
│   │   └── PublicController.cs                  // Public appointment lookup page 
│   │
│   ├── Models/                                  // View Models (UI-specific, not same as API models)
│   │   ├── AppointmentViewModel.cs              // Booking form and appointment display model
│   │   ├── DashboardViewModel.cs                // Dashboard summary data model
│   │   ├── DoctorScheduleViewModel.cs           // Schedule display and editing model
│   │   └── PatientHistoryViewModel.cs           // Patient visit and record history model
│   │
│   ├── Views/
│   │   ├── Appointment/
│   │   │   ├── Book.cshtml                      // Appointment booking form (step-by-step)
│   │   │   ├── Details.cshtml                   // Single appointment detail view
│   │   │   ├── List.cshtml                      // All appointments with filters
│   │   │   └── Edit.cshtml                      // Edit/reschedule appointment
│   │   │
│   │   ├── Doctor/
│   │   │   ├── Profile.cshtml                   // Doctor profile and bio
│   │   │   ├── Schedule.cshtml                  // Doctor's weekly schedule view
│   │   │   └── List.cshtml                      // All doctors with specialization filter
│   │   │
│   │   ├── Patient/
│   │   │   ├── Dashboard.cshtml                 // Patient homepage (upcoming + recent)
│   │   │   ├── History.cshtml                   // Full visit and record history
│   │   │   └── Profile.cshtml                   // Patient personal info and edit
│   │   │
│   │   ├── Dashboard/
│   │   │   ├── ReceptionistDashboard.cshtml     // Receptionist view (all appointments today)
│   │   │   └── ManagerDashboard.cshtml          // Manager overview (stats, links to reports)
│   │   │
│   │   ├── Account/
│   │   │   ├── Login.cshtml                     // Login page
│   │   │   └── Register.cshtml                  // New user registration
│   │   │
│   │   ├── Public/
│   │   │   └── Lookup.cshtml                    // Public appointment lookup (CPR + reference)
│   │   │
│   │   └── Shared/
│   │       ├── _Layout.cshtml                   // Main site layout (navbar, footer, scripts)
│   │       ├── _Navbar.cshtml                   // Role-aware navigation bar
│   │       ├── _Notifications.cshtml            // Notification dropdown partial
│   │       └── Error.cshtml                     // Error page
│   │
│   ├── wwwroot/                                 // Static files served directly to browser
│   │   ├── css/
│   │   │   ├── site.css                         // Custom styles layered on top of Bootstrap
│   │   │   └── dashboard.css                    // Dashboard-specific styles
│   │   ├── js/
│   │   │   ├── site.js                          // Global JS utilities and initialization
│   │   │   ├── appointments.js                  // AJAX booking, real-time status via SignalR
│   │   │   └── notifications.js                 // Notification polling and display
│   │   └── lib/
│   │       ├── bootstrap/                        // Bootstrap 5 CSS and JS
│   │       ├── jquery/                           // jQuery library
│   │       └── signalr/                          // SignalR client library
│   │
│   ├── Services/
│   │   └── ApiService.cs                        // HttpClient wrapper for calling Healthcare.API
│   │
│   ├── appsettings.json                         // API base URL, SignalR hub URL, Azure config
│   └── Program.cs                               // MVC startup, HttpClient config, SignalR client
│
│
├── Healthcare.Reporting/                        // Reporting Application
│   │
│   ├── Controllers/
│   │   └── ReportController.cs                  // Fetches and passes report data to views
│   │
│   ├── Views/
│   │   ├── Report/
│   │   │   ├── Index.cshtml                     // Reports home / navigation
│   │   │   ├── Appointments.cshtml              // Appointment statistics (counts, trends)
│   │   │   ├── DoctorWorkload.cshtml            // Per-doctor appointment load report
│   │   │   ├── Cancellations.cshtml             // Cancelled and missed appointment report
│   │   │   └── Patients.cshtml                  // Patient activity and engagement report
│   │   └── Shared/
│   │       └── _Layout.cshtml                   // Reporting app layout (minimal, read-only UI)
│   │
│   ├── Services/
│   │   └── ReportingApiService.cs               // All HttpClient calls to /api/reports/* endpoints
│   │
│   ├── appsettings.json                         // API base URL for reporting consumption
│   └── Program.cs                               // Reporting app startup (read-only, no write access)
│
│
└── Database/
    ├── schema.sql                               // Creates all tables, constraints, and relationships
    └── seed.sql                                 // Sample doctors, patients, and appointments for testing
```

---

## 🔌 API Documentation

> 🔒 Protected endpoints require `Authorization: Bearer <token>` header
> 🌐 Public endpoints require no authentication

---

### 🔐 Authentication & Identity

```
POST   /api/auth/login             // Login with credentials, returns JWT token        🌐
POST   /api/auth/register          // Register a new user account                      🌐
GET    /api/auth/me                // Get currently logged-in user info                🔒
```

---

### 📅 Appointments

```
GET    /api/appointments                          // Get all appointments               🔒
GET    /api/appointments/{id}                     // Get appointment by ID              🔒
GET    /api/appointments/doctor/{doctorId}        // Get all appointments for a doctor  🔒
GET    /api/appointments/patient/{patientId}      // Get all appointments for a patient 🔒
GET    /api/appointments/status/{status}          // Filter by status                   🔒
POST   /api/appointments                          // Book a new appointment             🔒
PUT    /api/appointments/{id}                     // Update appointment details/status  🔒
DELETE /api/appointments/{id}                     // Delete an appointment              🔒
```

> Status values: `Requested` → `Confirmed` → `Completed` / `Cancelled` / `No-Show`

---

### 👨‍⚕️ Doctors

```
GET    /api/doctors                               // Get all doctors                    🔒
GET    /api/doctors/{id}                          // Get doctor by ID                   🔒
GET    /api/doctors/{id}/schedule                 // Get doctor's available slots       🔒
GET    /api/doctors/specialization/{spec}         // Filter by specialization           🔒
POST   /api/doctors                               // Add new doctor (Manager only)      🔒
PUT    /api/doctors/{id}                          // Update doctor details              🔒
DELETE /api/doctors/{id}                          // Remove doctor (Manager only)       🔒
```

---

### 👤 Patients

```
GET    /api/patients                              // Get all patients                   🔒
GET    /api/patients/{id}                         // Get patient by ID                  🔒
GET    /api/patients/{id}/records                 // Get patient's full medical history 🔒
POST   /api/patients                              // Register a new patient             🔒
PUT    /api/patients/{id}                         // Update patient details             🔒
DELETE /api/patients/{id}                         // Delete patient record              🔒
```

---

### 📋 Medical Records & Prescriptions

```
GET    /api/records/{id}                          // Get medical record by ID           🔒
GET    /api/records/patient/{patientId}           // Get all records for a patient      🔒
POST   /api/records                               // Create record after appointment    🔒
GET    /api/prescriptions/patient/{patientId}     // Get all prescriptions for patient  🔒
POST   /api/prescriptions                         // Create prescription                🔒
```

---

### 🌐 Public Endpoints — No Login Required

```
GET    /api/public/appointments/{cpr}/{ref}       // Appointment lookup by CPR + ref    🌐
GET    /api/public/visits/{cpr}/{ref}             // Recent visit summary               🌐
```

---

### 📊 Reporting Endpoints — Read Only

```
GET    /api/reports/appointments                  // Appointment volume & trends        🔒
GET    /api/reports/doctors/workload              // Per-doctor appointment load        🔒
GET    /api/reports/cancellations                 // Cancelled and no-show data         🔒
GET    /api/reports/patients                      // Patient activity summary           🔒
```

---

## 🧩 System Features

| Feature | Description |
|---------|-------------|
| **Appointment Booking** | Book by specialization, doctor, date, and time slot with conflict checking |
| **Appointment Lifecycle** | Status flow: Requested → Confirmed → Completed / Cancelled / No-Show |
| **Doctor Scheduling** | Manage doctor availability, working days, and time slots |
| **Patient Records** | Full visit history with diagnosis, notes, and treatment per completed appointment |
| **Prescription Tracking** | Medication and dosage records linked to each visit |
| **Role-Based Access** | Separate views and permissions for Patient, Doctor, Receptionist, and Manager |
| **Public Lookup** | Any patient can check appointment status using CPR + reference — no login needed |
| **Real-Time Updates** | SignalR pushes live appointment status changes to the browser instantly |
| **Reporting** | Clinic managers view operational reports through a dedicated read-only reporting app |
| **Notifications** | In-app notifications for appointment confirmations, reminders, and updates |

---

## 🛠️ Technologies Used

| Technology | Purpose |
|-----------|---------|
| ASP.NET Core MVC | Main user-facing web application |
| ASP.NET Core Web API | Backend REST API and data services |
| Entity Framework Core | Database ORM and migrations |
| SQL Server | Relational database |
| JWT (JSON Web Tokens) | Authentication and role-based authorization |
| SignalR | Real-time appointment status updates |
| Bootstrap 5 | UI layout and responsive design |
| jQuery / AJAX | Interactive forms and dynamic content loading |
| HttpClient | API communication from MVC and Reporting apps |
| Azure App Service | Cloud hosting for all three projects |
| Azure SQL Database | Cloud-hosted production database |

---

## ☁️ Deployment

| Component | Platform |
|----------|----------|
| Healthcare.MVC | Azure App Service |
| Healthcare.API | Azure App Service |
| Healthcare.Reporting | Azure App Service |
| Database | Azure SQL Database |

> Public URLs for all three will be provided in the final submission.

---

## 📌 Architecture Notes

- The **MVC application** is the primary system used by patients, doctors, and receptionists daily.
- The **Web API** is the single source of truth — both MVC and Reporting consume it via HttpClient.
- The **Reporting App** is completely read-only — it never writes to the database directly; it only calls `GET /api/reports/*` endpoints.
- **SignalR** is hosted inside the API and connected to from the MVC client for real-time updates.
- The system enforces **role-based access control** — users only see and access what their role permits.
- The **public lookup feature** requires no login and is intentionally separated into `/api/public/*` endpoints with no JWT protection.
