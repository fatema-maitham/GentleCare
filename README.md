# GentleCare

Care that feels personal, simple, and close to you

GentleCare is a modern healthcare clinic management system built with ASP.NET Core MVC, Web API, and a Reporting Application for managing appointments, doctors, patients, schedules, medical records, notifications, and clinic reports.

---

## Team Members

| Student ID | Name |
|---|---|
| 202304661 | Fatema Maitham |
| 202305590 | Maram Shubbar |
| 202300641 | Malak Almajed |
| 202302211 | Zainab Almahdi |
| 202302702 | Kawther Abdulla |

---

## Roles and Responsibilities

| Member | Role | Work Owned |
|---|---|---|
| Fatema Maitham | MVC Developer | Clinic Manager MVC features including dashboard, doctor management, schedule management, appointment management, reports dashboard, account activation/deactivation controls, clinic announcements, doctor follow-up request workflow, and appointment workflow improvements. |
| Maram Shubbar | MVC Developer | Patient MVC pages including dashboard, booking, appointments, visit history, prescriptions, profile management. Receptionist MVC pages including dashboard, booking, appointment status management, live queue. Doctor MVC pages for appointment viewing and status updates. Role-based dashboards, authentication-related MVC views, and clinic workflow features. |
| Malak Almajed | API and Backend Developer | ERD design, database schema planning, EF Core database layer and entity relationships, RESTful Web API design and implementation, JWT authentication and token service, public appointment lookup endpoint, API security, and backend integration with MVC application. |
| Zainab Almahdi | UI/UX, Testing, Deployment and Documentation | Layout styling, responsiveness across all devices, navigation improvements, consistent design system, overall user experience enhancement, visual design for all role-based interfaces, system testing, Azure deployment support, screenshots, and project documentation. |
| Kawther Abdulla | Reporting Developer | Reporting application development, HttpClient API consumption for data retrieval, report views and dashboards, and read-only reporting enforcement for secure data access. |

---

## System Roles

| Role | Description |
|---|---|
| Clinic Manager | Manages doctors, schedules, leaves, appointments, accounts, reports, notifications, and announcements. |
| Receptionist | Books appointments, manages appointment flow, handles check-ins, and monitors appointment queue. |
| Doctor | Views assigned appointments, updates status, creates visit records, records prescriptions, and creates follow-up requests. |
| Patient | Books appointments, views upcoming appointments, visit history, prescriptions, and notifications. |

---

## ERD

The ERD represents the main GentleCare database structure, including users, roles, doctors, patients, appointments, schedules, leaves, specializations, visit records, prescriptions, and notifications.

![GentleCare ERD](MVCApp/wwwroot/images/gentlecare-erd.jpeg)

---

## Database Design Summary

| Table                 | Purpose                                                                                                               |
| --------------------- | --------------------------------------------------------------------------------------------------------------------- |
| AspNetUsers           | Stores login users, full names, emails, passwords, profile pictures, active status, and created date.                 |
| AspNetRoles           | Stores system roles such as Clinic Manager, Doctor, Receptionist, and Patient.                                        |
| AspNetUserRoles       | Links users to roles.                                                                                                 |
| AspNetUserClaims      | Stores user claims used by ASP.NET Identity.                                                                          |
| AspNetRoleClaims      | Stores role claims used by ASP.NET Identity.                                                                          |
| AspNetUserLogins      | Stores external login information if used.                                                                            |
| AspNetUserTokens      | Stores authentication tokens if used.                                                                                 |
| Doctors               | Stores doctor profile data linked to an Identity user.                                                                |
| Patients              | Stores patient profile data, CPR number, reference number, date of birth, blood type, address, and emergency contact. |
| Specializations       | Stores clinic specialization names and descriptions.                                                                  |
| DoctorSpecializations | Many-to-many table linking doctors to multiple specializations.                                                       |
| DoctorSchedules       | Stores doctor working day, start time, end time, and slot duration.                                                   |
| DoctorLeaves          | Stores doctor leave periods and leave reasons.                                                                        |
| AppointmentStatuses   | Stores appointment workflow statuses.                                                                                 |
| Appointments          | Stores appointment date, time, patient, doctor, status, notes, cancellation reason, and timestamps.                   |
| VisitRecords          | Stores doctor notes, diagnosis, treatment, and completed visit details.                                               |
| Prescriptions         | Stores medication name, dosage, frequency, duration, and instructions linked to visit records.                        |
| NotificationTypes     | Stores notification categories.                                                                                       |
| Notifications         | Stores in-system notifications for users.                                                                             |

---

## Detailed Project Structure

```text
GentleCare/
│
├── GentleCare.sln                                 // Visual Studio solution file
├── README.md                                      // Project documentation
├── schema.sql                                     // SQL script for database schema
├── seed.sql                                       // SQL script for seeded test data
├── .gitignore                                     // Git ignored files configuration
├── .gitattributes                                 // Git repository attributes
│
├── WebAPI/                                        // ASP.NET Core Web API and shared data layer
│   │
│   ├── WebAPI.csproj                              // Web API project file
│   ├── Program.cs                                 // API startup, services, middleware, authentication, CORS, and SignalR
│   ├── appsettings.json                           // API configuration and connection string
│   ├── appsettings.Development.json               // Development configuration
│   ├── WebAPI.http                                // API testing requests
│   │
│   ├── Controllers/                               // API controllers
│   │   ├── AppointmentController.cs               // Appointment API endpoints
│   │   ├── AuthController.cs                      // Login and JWT authentication endpoints
│   │   ├── DoctorController.cs                    // Doctor-related API endpoints
│   │   ├── PatientController.cs                   // Patient-related API endpoints
│   │   └── ReportController.cs                    // Report data endpoints
│   │
│   ├── DTO/                                       // API data transfer objects
│   │   ├── AppointmentDTOs.cs                     // Appointment request and response DTOs
│   │   ├── AuthDTOs.cs                            // Authentication DTOs
│   │   ├── DoctorDTOs.cs                          // Doctor DTOs
│   │   ├── PatientDTOs.cs                         // Patient DTOs
│   │   └── ReportDTOs.cs                          // Report DTOs
│   │
│   ├── Data/                                      // Database configuration and seeding
│   │   ├── ApplicationDbContext.cs                // EF Core DbContext and database relationships
│   │   └── DbSeeder.cs                            // Seeds users, roles, doctors, patients, statuses, notifications, and test data
│   │
│   ├── Hubs/                                      // SignalR hubs
│   │   └── AppointmentHub.cs                      // Real-time appointment and queue update hub
│   │
│   ├── Migrations/                                // EF Core migrations
│   │   ├── 20260401193514_InitialCreate.cs        // Initial database migration
│   │   ├── 20260408104851_ImprovedSchema.cs       // Improved schema migration
│   │   ├── 20260410191344_LookupTables.cs         // Lookup tables migration
│   │   └── ApplicationDbContextModelSnapshot.cs   // Current EF Core model snapshot
│   │
│   ├── Models/                                    // Database entity models
│   │   ├── ApplicationUser.cs                     // Identity user extension with profile and active status
│   │   ├── Appointment.cs                         // Appointment entity with patient, doctor, status, date, and time
│   │   ├── AppointmentStatusLookup.cs             // Appointment status lookup entity
│   │   ├── Doctor.cs                              // Doctor profile entity
│   │   ├── DoctorLeave.cs                         // Doctor leave entity
│   │   ├── DoctorSchedule.cs                      // Doctor weekly schedule entity
│   │   ├── DoctorSpecialization.cs                // Doctor and specialization link entity
│   │   ├── Notification.cs                        // User notification entity
│   │   ├── NotificationType.cs                    // Notification type lookup entity
│   │   ├── Patient.cs                             // Patient profile entity
│   │   ├── Prescription.cs                        // Prescription entity linked to visit records
│   │   ├── Specialization.cs                      // Medical specialization entity
│   │   └── VisitRecord.cs                         // Visit record entity for completed appointments
│   │
│   ├── Properties/                                // Project launch settings
│   │   └── launchSettings.json                    // Local run profiles
│   │
│   └── Services/                                  // API services
│       ├── NotificationHubService.cs              // Sends real-time notification updates through SignalR
│       └── TokenService.cs                        // Creates JWT tokens for authenticated users
│
│
├── MVCApp/                                        // ASP.NET Core MVC user-facing application
│   │
│   ├── MVCApp.csproj                              // MVC project file
│   ├── Program.cs                                 // MVC startup, services, Identity, EF Core, routing, and session
│   ├── appsettings.json                           // MVC configuration and connection string
│   ├── appsettings.Development.json               // Development configuration
│   │
│   ├── Controllers/                               // MVC page controllers
│   │   ├── AccountController.cs                   // Login, register, logout, and access denied logic
│   │   ├── AppointmentController.cs               // General appointment pages and appointment-related actions
│   │   ├── ClinicManagerController.cs             // Clinic Manager dashboard, doctors, schedules, reports, accounts, announcements
│   │   ├── DashboardController.cs                 // Role-based dashboard routing
│   │   ├── DoctorAppointmentController.cs         // Doctor appointments, details, status updates, history, and follow-up requests
│   │   ├── DoctorController.cs                    // Doctor dashboard, schedule, profile, and notifications
│   │   ├── PatientController.cs                   // Patient dashboard, booking, appointments, history, notifications, and profile
│   │   ├── PrescriptionController.cs              // Prescription pages and actions
│   │   ├── PublicController.cs                    // Public appointment lookup page using API
│   │   ├── ReceptionistController.cs              // Receptionist booking, appointment workflow, patient search, and live queue
│   │   └── VisitRecordController.cs               // Visit record creation and editing
│   │
│   ├── Models/                                    // General MVC models
│   │   └── ErrorViewModel.cs                      // Error page model
│   │
│   ├── Services/                                  // MVC business logic services
│   │   ├── AppointmentWorkflowService.cs          // Appointment status workflow and valid transition rules
│   │   ├── ClinicManagerService.cs                // Clinic Manager business logic
│   │   ├── ClinicNotificationService.cs           // Clinic announcement and role notification logic
│   │   ├── DoctorAppointmentService.cs            // Doctor appointment, history, visit, prescription, and follow-up logic
│   │   ├── DoctorDashboardService.cs              // Doctor dashboard, schedule, profile, and notification logic
│   │   ├── NotificationService.cs                 // Creates and reads in-system notifications
│   │   ├── PatientService.cs                      // Patient dashboard, booking, profile, notifications, and history logic
│   │   ├── PrescriptionService.cs                 // Prescription business logic
│   │   ├── ReceptionistService.cs                 // Receptionist booking, queue, patient search, and appointment workflow logic
│   │   └── VisitRecordService.cs                  // Visit record and treatment logic
│   │
│   ├── Services/Interfaces/                       // Service contracts
│   │   ├── IAppointmentWorkflowService.cs         // Appointment workflow service interface
│   │   ├── IClinicManagerService.cs               // Clinic Manager service interface
│   │   ├── IClinicNotificationService.cs          // Clinic notification and announcement service interface
│   │   ├── IDoctorAppointmentService.cs           // Doctor appointment service interface
│   │   ├── IDoctorDashboardService.cs             // Doctor dashboard service interface
│   │   ├── INotificationService.cs                // Notification service interface
│   │   ├── IPatientService.cs                     // Patient service interface
│   │   ├── IPrescriptionService.cs                // Prescription service interface
│   │   ├── IReceptionistService.cs                // Receptionist service interface
│   │   └── IVisitRecordService.cs                 // Visit record service interface
│   │
│   ├── ViewModels/                                // Strongly typed page models
│   │   ├── LoginViewModel.cs                      // Login form model
│   │   ├── RegisterViewModel.cs                   // Registration form model
│   │   │
│   │   ├── Appointment/                           // Appointment view models
│   │   │   ├── CreatePrescriptionViewModel.cs     // Create prescription model
│   │   │   ├── CreateVisitRecordViewModel.cs      // Create visit record model
│   │   │   ├── DoctorAppointmentDetailsViewModel.cs       // Doctor appointment details model
│   │   │   ├── DoctorAppointmentListItemViewModel.cs      // Doctor appointment row model
│   │   │   └── UpdateAppointmentStatusViewModel.cs        // Appointment status update model
│   │   │
│   │   ├── ClinicManager/                         // Clinic Manager view models
│   │   │   ├── AppointmentImpactViewModel.cs      // Affected appointment review model
│   │   │   ├── AppointmentRescheduleSuggestionViewModel.cs // Suggested replacement slot model
│   │   │   ├── ClinicAnnouncementViewModel.cs     // Announcement creation model
│   │   │   ├── ClinicManagerAppointmentDetailsViewModel.cs // Manager appointment details model
│   │   │   ├── ClinicManagerAppointmentItemViewModel.cs    // Manager appointment row model
│   │   │   ├── ClinicManagerAppointmentStatusViewModel.cs  // Manager appointment status model
│   │   │   ├── ClinicManagerAppointmentsViewModel.cs       // Manager appointment list model
│   │   │   ├── ClinicManagerDashboardViewModel.cs          // Manager dashboard model
│   │   │   ├── ClinicManagerDoctorDetailsViewModel.cs      // Doctor details model
│   │   │   ├── ClinicManagerDoctorListItemViewModel.cs     // Doctor row model
│   │   │   ├── ClinicManagerDoctorListViewModel.cs         // Doctor list page model
│   │   │   ├── ClinicManagerNotificationViewModel.cs       // Manager notification model
│   │   │   ├── ClinicManagerProfileViewModel.cs            // Manager profile model
│   │   │   ├── ClinicManagerUserAccountsViewModel.cs       // Account activation/deactivation model
│   │   │   ├── ClinicReportItemViewModel.cs                // Doctor utilization report row model
│   │   │   ├── ClinicReportViewModel.cs                    // Full reports dashboard model
│   │   │   ├── DoctorCreateViewModel.cs                    // Create doctor form model
│   │   │   ├── DoctorEditViewModel.cs                      // Edit doctor form model
│   │   │   ├── DoctorLeaveFormViewModel.cs                 // Doctor leave form model
│   │   │   ├── DoctorLeaveItemViewModel.cs                 // Doctor leave row model
│   │   │   ├── DoctorScheduleFormViewModel.cs              // Doctor schedule form model
│   │   │   ├── DoctorScheduleItemViewModel.cs              // Doctor schedule row model
│   │   │   ├── EditClinicManagerProfileViewModel.cs        // Edit manager profile model
│   │   │   ├── ImpactedAppointmentItemViewModel.cs         // Impacted appointment row model
│   │   │   ├── ManageDoctorLeavesViewModel.cs              // Manage doctor leaves page model
│   │   │   ├── ManageDoctorScheduleViewModel.cs            // Manage doctor schedules page model
│   │   │   ├── ManageDoctorSpecializationsViewModel.cs     // Manage doctor specializations page model
│   │   │   └── SpecializationSelectionViewModel.cs         // Specialization checkbox model
│   │   │
│   │   ├── Doctor/                                // Doctor view models
│   │   │   ├── CreateFollowUpRequestViewModel.cs  // Follow-up appointment request form model
│   │   │   ├── CreateVisitRecordViewModel.cs      // Create visit record model
│   │   │   ├── DoctorAppointmentDetailsViewModel.cs       // Doctor appointment details model
│   │   │   ├── DoctorAppointmentListItemViewModel.cs      // Doctor appointment row model
│   │   │   ├── DoctorAppointmentListViewModel.cs          // Doctor appointment list page model
│   │   │   ├── DoctorDashboardViewModel.cs                // Doctor dashboard model
│   │   │   ├── DoctorNotificationsViewModel.cs            // Doctor notifications model
│   │   │   ├── DoctorPatientHistoryViewModel.cs           // Patient history model for doctor
│   │   │   ├── DoctorPrescriptionViewModel.cs             // Doctor prescription display model
│   │   │   ├── DoctorProfileViewModel.cs                  // Doctor profile model
│   │   │   ├── DoctorScheduleItemViewModel.cs             // Doctor schedule row model
│   │   │   ├── DoctorScheduleViewModel.cs                 // Doctor schedule page model
│   │   │   ├── EditDoctorProfileViewModel.cs              // Edit doctor profile model
│   │   │   ├── EditVisitRecordViewModel.cs                // Edit visit record model
│   │   │   ├── PrescriptionInputViewModel.cs              // Prescription input model
│   │   │   ├── UpdateAppointmentStatusViewModel.cs        // Doctor appointment status update model
│   │   │   └── VisitRecordDetailsViewModel.cs             // Visit record details model
│   │   │
│   │   ├── Patient/                               // Patient view models
│   │   │   ├── PatientAppointmentsViewModel.cs    // Patient appointment list model
│   │   │   ├── PatientBookAppointmentViewModel.cs // Patient booking form model
│   │   │   ├── PatientDashboardViewModel.cs       // Patient dashboard model
│   │   │   ├── PatientEditProfileViewModel.cs     // Edit patient profile model
│   │   │   ├── PatientHistoryViewModel.cs         // Patient visit history model
│   │   │   ├── PatientNotificationViewModel.cs    // Patient notification model
│   │   │   ├── PatientProfileViewModel.cs         // Patient profile model
│   │   │   └── PrescriptionItemViewModel.cs       // Prescription row model
│   │   │
│   │   ├── Public/                                // Public lookup view models
│   │   │   └── PublicAppointmentLookupViewModel.cs // Public appointment lookup result model
│   │   │
│   │   └── Receptionist/                          // Receptionist view models
│   │       ├── ReceptionistAppointmentListItemViewModel.cs     // Receptionist appointment row model
│   │       ├── ReceptionistAppointmentsPageViewModel.cs        // Receptionist appointment page model
│   │       ├── ReceptionistBookAppointmentViewModel.cs         // Receptionist booking model
│   │       ├── ReceptionistDashboardViewModel.cs               // Receptionist dashboard model
│   │       ├── ReceptionistLiveQueueItemViewModel.cs           // Live queue row model
│   │       ├── ReceptionistLiveQueueViewModel.cs               // Live queue page model
│   │       ├── ReceptionistPatientSearchResultViewModel.cs     // Patient search result model
│   │       ├── ReceptionistPatientSearchViewModel.cs           // Patient search page model
│   │       └── ReceptionistUpdateAppointmentStatusViewModel.cs // Receptionist status update model
│   │
│   ├── Views/                                     // Razor views
│   │   ├── Account/                               // Account pages
│   │   │   ├── AccessDenied.cshtml                // Access denied page
│   │   │   ├── Login.cshtml                       // Login page
│   │   │   └── Register.cshtml                    // Register page
│   │   │
│   │   ├── Appointment/                           // General appointment pages
│   │   │   ├── AddPrescription.cshtml             // Add prescription page
│   │   │   ├── Book.cshtml                        // Appointment booking page
│   │   │   ├── CreateVisitRecord.cshtml           // Create visit record page
│   │   │   ├── Details.cshtml                     // Appointment details page
│   │   │   ├── List.cshtml                        // Appointment list page
│   │   │   ├── MyAppointments.cshtml              // Current user's appointments page
│   │   │   └── PatientHistory.cshtml              // Patient history page
│   │   │
│   │   ├── ClinicManager/                         // Clinic Manager pages
│   │   │   ├── AppointmentDetails.cshtml          // Manager appointment details page
│   │   │   ├── AppointmentImpact.cshtml           // Affected appointment review page
│   │   │   ├── Appointments.cshtml                // Manager appointment list page
│   │   │   ├── CreateAnnouncement.cshtml          // Manager announcement form page
│   │   │   ├── CreateDoctor.cshtml                // Create doctor page
│   │   │   ├── CreateDoctorLeave.cshtml           // Create doctor leave page
│   │   │   ├── CreateDoctorSchedule.cshtml        // Create doctor schedule page
│   │   │   ├── Dashboard.cshtml                   // Manager dashboard page
│   │   │   ├── DoctorDetails.cshtml               // Doctor details page
│   │   │   ├── Doctors.cshtml                     // Doctor list page
│   │   │   ├── EditDoctor.cshtml                  // Edit doctor page
│   │   │   ├── EditDoctorLeave.cshtml             // Edit doctor leave page
│   │   │   ├── EditDoctorSchedule.cshtml          // Edit doctor schedule page
│   │   │   ├── EditProfile.cshtml                 // Edit manager profile page
│   │   │   ├── ManageDoctorLeaves.cshtml          // Manage doctor leaves page
│   │   │   ├── ManageDoctorSchedule.cshtml        // Manage doctor schedules page
│   │   │   ├── ManageDoctorSpecializations.cshtml // Manage doctor specializations page
│   │   │   ├── Notifications.cshtml               // Manager notifications page
│   │   │   ├── Profile.cshtml                     // Manager profile page
│   │   │   ├── Reports.cshtml                     // Manager reports dashboard
│   │   │   ├── UpdateAppointmentStatus.cshtml     // Manager appointment status update page
│   │   │   └── UserAccounts.cshtml                // User activation and deactivation page
│   │   │
│   │   ├── Dashboard/                             // Shared dashboard routing
│   │   │   └── Index.cshtml                       // Redirects users to the correct role dashboard
│   │   │
│   │   ├── Doctor/                                // Doctor pages
│   │   │   ├── AppointmentDetails.cshtml          // Doctor appointment details page
│   │   │   ├── Appointments.cshtml                // Doctor appointment list page
│   │   │   ├── CreateFollowUpRequest.cshtml       // Follow-up request form page
│   │   │   ├── CreateVisitRecord.cshtml           // Create visit record page
│   │   │   ├── Dashboard.cshtml                   // Doctor dashboard page
│   │   │   ├── DoctorAppointmentStatusViewModel.cs // Doctor appointment status support view file
│   │   │   ├── EditProfile.cshtml                 // Edit doctor profile page
│   │   │   ├── EditVisitRecord.cshtml             // Edit visit record page
│   │   │   ├── Notifications.cshtml               // Doctor notifications page
│   │   │   ├── PatientHistory.cshtml              // Patient history page for doctor
│   │   │   ├── Prescriptions.cshtml               // Doctor prescriptions page
│   │   │   ├── Profile.cshtml                     // Doctor profile page
│   │   │   ├── Schedule.cshtml                    // Doctor schedule page
│   │   │   └── UpdateStatus.cshtml                // Doctor appointment status update page
│   │   │
│   │   ├── Patient/                               // Patient pages
│   │   │   ├── Appointments.cshtml                // Patient appointments page
│   │   │   ├── BookAppointment.cshtml             // Patient booking page
│   │   │   ├── Dashboard.cshtml                   // Patient dashboard page
│   │   │   ├── EditProfile.cshtml                 // Edit patient profile page
│   │   │   ├── History.cshtml                     // Patient visit history page
│   │   │   ├── Notifications.cshtml               // Patient notifications page
│   │   │   └── Profile.cshtml                     // Patient profile page
│   │   │
│   │   ├── Public/                                // Public pages
│   │   │   └── Lookup.cshtml                      // Public appointment lookup page
│   │   │
│   │   ├── Receptionist/                          // Receptionist pages
│   │   │   ├── Appointments.cshtml                // Receptionist appointments page
│   │   │   ├── BookAppointment.cshtml             // Receptionist booking page
│   │   │   ├── Index.cshtml                       // Receptionist dashboard page
│   │   │   ├── LiveQueue.cshtml                   // Receptionist live queue page
│   │   │   ├── PatientSearch.cshtml               // Receptionist patient search page
│   │   │   └── UpdateStatus.cshtml                // Receptionist status update page
│   │   │
│   │   └── Shared/                                // Shared layout and partial views
│   │       ├── _AlertMessages.cshtml              // Shared alert message partial
│   │       ├── Error.cshtml                       // Error page
│   │       ├── _Layout.cshtml                     // Main site layout
│   │       ├── _Layout.cshtml.css                 // Layout-scoped styles
│   │       ├── _Navbar.cshtml                     // Role-aware navigation bar
│   │       └── _ValidationScriptsPartial.cshtml   // Client-side validation scripts
│   │
│   ├── wwwroot/                                   // Static web assets
│   │   ├── favicon.ico                            // Browser icon
│   │   ├── logo.png                               // GentleCare logo
│   │   ├── css/
│   │   │   └── site.css                           // Main site CSS
│   │   ├── js/
│   │   │   └── site.js                            // Main site JavaScript
│   │   ├── images/                                // Uploaded and static images
│   │   │   ├── gentlecare-erd.jpeg                // ERD image used in README
│   │   │   ├── default-patient.png                // Default patient image
│   │   │   ├── doctors/                           // Doctor profile images
│   │   │   ├── managers/                          // Manager profile images
│   │   │   └── patients/                          // Patient profile images
│   │   └── lib/                                   // Client libraries such as Bootstrap, jQuery, and validation scripts
│   │
│   └── Properties/
│       └── launchSettings.json                    // MVC local run profiles
```

---

## System Features

| Feature               | Description                                                                                                           |
| --------------------- | --------------------------------------------------------------------------------------------------------------------- |
| Appointment Booking   | Patients or receptionists can book appointments by specialization, doctor, date, and available time slot.             |
| Appointment Lifecycle | Appointments follow a workflow from Requested to Confirmed, Checked-In, In Progress, Completed, Cancelled, or Missed. |
| Doctor Scheduling     | Clinic Manager manages doctor working days, working hours, slot duration, and leave periods.                          |
| Patient Records       | Doctors create visit records for completed appointments, including notes, diagnosis, and treatment.                   |
| Prescription Tracking | Prescriptions are linked to visit records and can be viewed by patients and doctors.                                  |
| Role-Based Access     | Each role has its own pages and permissions.                                                                          |
| Public Lookup         | Patients can check upcoming appointments and recent visit summary using CPR and reference number without logging in.  |
| Real-Time Updates     | SignalR supports live queue updates and appointment status changes.                                                   |
| Reporting             | Clinic Manager can view advanced operational reports.                                                                 |
| Notifications         | Users receive in-system notifications for appointment and clinic events.                                              |

---

## Clinic Manager MVC Features

| Feature                    | Description                                                                                       |
| -------------------------- | ------------------------------------------------------------------------------------------------- |
| Manager Dashboard          | Shows clinic summary, appointment counts, doctor workload, and quick access to management pages.  |
| Doctor Management          | Manager can create, edit, view, activate, and manage doctor profiles.                             |
| Doctor Specializations     | Manager can assign multiple specializations to doctors.                                           |
| Doctor Schedule Management | Manager can create, edit, and delete doctor weekly schedules.                                     |
| Doctor Leave Management    | Manager can create, edit, and delete doctor leave periods.                                        |
| Appointment Impact Review  | Manager can review appointments affected by doctor schedule or leave changes.                     |
| Appointment Management     | Manager can view clinic appointments, filter them, open details, and update appointment statuses. |
| Reports Dashboard          | Manager can view advanced operational reports for clinic decision-making.                         |
| Clinic Announcements       | Manager can send announcements to doctors, receptionists, patients, or all users.                 |
| User Account Controls      | Manager can activate or deactivate doctor, receptionist, and patient accounts.                    |
| Notifications              | Manager can view notifications and mark them as read.                                             |
| Profile Management         | Manager can view and update their profile information and profile picture.                        |

---

## Advanced Features

| # | Advanced Feature | Description |
|---|---|---|
| 1 | Clinic Announcement System | Allows the Clinic Manager to send announcements to doctors, receptionists, patients, or all users for clinic-wide communication. |
| 2 | Doctor Follow-Up Appointment Request | Allows doctors to request a follow-up appointment after a completed visit to support continuity of care. |
| 3 | Manager Account Activation and Deactivation | Allows the Clinic Manager to activate or deactivate doctor, receptionist, and patient accounts. Inactive users cannot log in. |
| 4 | Smart Rescheduling and Leave Impact Handling | Shows appointments affected by doctor leave or schedule changes, suggests replacement slots based on availability, conflicts, leave periods, and specializations, and re-validates the selected slot before rescheduling. |
| 5 | Enhanced Clinic Reports Dashboard | Provides advanced reports including monthly performance, doctor utilization, specialization demand, busiest hours, doctor leave impact, missed appointment risk, cancellation reason analysis, and prescription volume. |
| 6 | Service Layer Architecture | Uses service classes and Dependency Injection to keep controllers clean, separate business logic from controller actions, and improve maintainability and reusability. |

---

## Services Layer

The MVC application uses a service layer to keep business logic separate from controllers and views. Services handle appointment workflows, dashboards, profiles, schedules, notifications, reports, announcements, and role-specific operations.

| # | Service | Purpose |
|---|---------|---------|
| 1 | Account Service | Handles login, logout, registration, role-based access, and account-related logic. |
| 2 | Patient Service | Handles patient dashboard, profile, appointment booking, appointment history, and patient-related data. |
| 3 | Doctor Dashboard Service | Handles doctor dashboard data, profile details, schedules, appointments, notifications, and profile updates. |
| 4 | Doctor Appointment Service | Handles doctor appointment lists, appointment details, status updates, and appointment workflow actions. |
| 5 | Visit Record Service | Handles creating and viewing visit records for completed patient appointments. |
| 6 | Prescription Service | Handles doctor prescriptions and prescription history linked to patient visits. |
| 7 | Receptionist Service | Handles receptionist dashboard, queue management, patient check-in, appointment flow, and receptionist operations. |
| 8 | Clinic Manager Doctor Service | Handles doctor listing, doctor details, doctor creation, doctor editing, and doctor account management. |
| 9 | Clinic Manager Schedule Service | Handles doctor schedules, working hours, leave management, and schedule changes. |
| 10 | Clinic Manager Report Service | Handles clinic reports such as appointment performance, doctor utilization, specialization demand, cancellation analysis, and prescription volume. |
| 11 | Appointment Workflow Service | Controls valid appointment status transitions and prevents invalid workflow changes. |
| 12 | Notification Service | Creates and manages notifications for patients, doctors, receptionists, and managers. |
| 13 | Announcement Service | Handles clinic announcements sent by the Clinic Manager to selected roles or all users. |
| 14 | Public Lookup Service | Allows public appointment lookup using CPR number and patient reference number without login. |

---

## Doctor MVC Features

| Feature             | Description                                                                 |
| ------------------- | --------------------------------------------------------------------------- |
| Doctor Dashboard    | Shows doctor appointments, schedule, and important updates.                 |
| Appointment List    | Doctor can view assigned appointments with search and filters.              |
| Appointment Details | Doctor can view patient and appointment information.                        |
| Status Updates      | Doctor can update appointment status based on allowed workflow transitions. |
| Visit Records       | Doctor can create or edit visit records for completed appointments.         |
| Prescriptions       | Doctor can record prescriptions linked to visit records.                    |
| Patient History     | Doctor can view visit history for patients they treated.                    |
| Follow-Up Request   | Doctor can create a follow-up appointment request after a completed visit.  |
| Notifications       | Doctor can view appointment and clinic notifications.                       |
| Profile Management  | Doctor can view and update profile information and profile picture.         |

---

## Patient MVC Features

| Feature            | Description                                                                        |
| ------------------ | ---------------------------------------------------------------------------------- |
| Patient Dashboard  | Shows patient summary, upcoming appointments, and recent activity.                 |
| Book Appointment   | Patient can book appointments by specialization, doctor, date, and available time. |
| My Appointments    | Patient can view upcoming and past appointments.                                   |
| Visit History      | Patient can view completed visit records.                                          |
| Prescriptions      | Patient can view prescriptions linked to their visits.                             |
| Notifications      | Patient receives appointment and clinic-related notifications.                     |
| Profile Management | Patient can view and update profile information.                                   |

---

## Receptionist MVC Features

| Feature                       | Description                                                                                                           |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| Receptionist Dashboard        | Shows daily appointment activity and clinic queue summary.                                                            |
| Appointment Booking           | Receptionist can book appointments for patients.                                                                      |
| Appointment Status Management | Receptionist can update appointment flow such as confirmed, checked-in, in progress, completed, cancelled, or missed. |
| Live Queue                    | Receptionist can monitor appointment progress and queue status.                                                       |
| Notifications                 | Receptionist receives relevant appointment updates and clinic announcements.                                          |

---

## Public Feature

| Feature                   | Description                                                                                                                           |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Public Appointment Lookup | Patients can enter CPR number and patient reference number without logging in to view upcoming appointments and recent visit summary. |

---

## API Endpoints

| Route | Method | Auth | Purpose |
| --- | --- | --- | --- |
| `/api/Auth/register` | POST | None | Register a new user account. |
| `/api/Auth/login` | POST | None | Login and return JWT token. |
| `/api/Appointment/lookup` | GET | None | Public patient lookup by CPR and reference number. |
| `/api/Appointment` | GET | JWT + Receptionist / Clinic Manager | Get all appointments. |
| `/api/Appointment/my` | GET | JWT + Patient | Get current patient's appointments. |
| `/api/Appointment/{id}/status` | PUT | JWT + Doctor / Receptionist / Clinic Manager | Update appointment status. |
| `/api/Doctor` | GET | JWT | Get all active doctors. |
| `/api/Doctor` | POST | JWT + Clinic Manager | Create a new doctor profile. |
| `/api/Doctor/{id}` | GET | JWT | Get doctor details by ID. |
| `/api/Doctor/{id}/availability` | GET | JWT | Get available time slots for a doctor on a date. |
| `/api/Patient` | POST | JWT + Patient / Receptionist / Clinic Manager | Create a patient profile. |
| `/api/Patient` | GET | JWT + Receptionist / Clinic Manager | Get all patients. |
| `/api/Patient/{id}` | GET | JWT + Doctor / Receptionist / Clinic Manager | Get patient details by ID. |
| `/api/Patient/{id}` | PUT | JWT + Patient / Receptionist / Clinic Manager | Update patient information. |
| `/api/Patient/my` | GET | JWT + Patient | Get the current patient's profile. |
| `/api/Patient/{id}/history` | GET | JWT + Doctor / Receptionist / Clinic Manager | Get patient visit history with prescriptions. |
| `/api/Report/appointment-stats` | GET | JWT + Clinic Manager | Appointment statistics by status and date range. |
| `/api/Report/doctor-workload` | GET | JWT + Clinic Manager | Doctor workload and appointment distribution. |
| `/api/Report/specialization-stats` | GET | JWT + Clinic Manager | Appointment statistics grouped by specialization. |
| `/api/Report/daily-summary` | GET | JWT + Clinic Manager | Daily clinic appointment summary. |
---

## Routing Table

The system uses ASP.NET Core MVC routes for the web application and Web API routes for API access.  
Access control is handled through ASP.NET Core Identity roles and JWT authentication where required.

---

## MVC Routes

### Public

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/` | Public | Display the public appointment lookup page. |
| GET | `/Public/Lookup` | Public | Display the public appointment lookup form. |
| POST | `/Public/Lookup` | Public | Search appointments using patient lookup details. |

---

### Account

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/Account/Login` | Public | Display the login form. |
| POST | `/Account/Login` | Public | Authenticate the user and sign them in. |
| GET | `/Account/Register` | Public | Display the patient registration form. |
| POST | `/Account/Register` | Public | Create a new patient account. |
| GET | `/Account/AccessDenied` | Public | Display the access denied page. |
| POST | `/Account/Logout` | Authenticated | Log out the current user. |

---

### Patient

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/Patient/Dashboard` | Patient | Display the patient dashboard. |
| GET | `/Patient/Profile` | Patient | Display the current patient's profile. |
| GET | `/Patient/EditProfile` | Patient | Display the edit profile form. |
| POST | `/Patient/EditProfile` | Patient | Update the current patient's profile. |
| GET | `/Patient/Appointments` | Patient | Display the patient's appointments. |
| GET | `/Patient/History` | Patient | Display the patient's medical history. |
| GET | `/Patient/Notifications` | Patient | Display patient notifications. |
| GET | `/Patient/BookAppointment` | Patient | Display the appointment booking form. |
| POST | `/Patient/BookAppointment` | Patient | Create a new appointment request. |
| POST | `/Patient/CancelAppointment` | Patient | Cancel an appointment when cancellation is allowed. |

---

### Receptionist

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/Receptionist/Index` | Receptionist | Display the receptionist dashboard. |
| GET | `/Receptionist/Appointments` | Receptionist | Display and filter appointments. |
| GET | `/Receptionist/BookAppointment` | Receptionist | Display the appointment booking form. |
| POST | `/Receptionist/BookAppointment` | Receptionist | Book an appointment for a patient. |
| GET | `/Receptionist/UpdateStatus` | Receptionist | Display the appointment status update form. |
| POST | `/Receptionist/UpdateStatus` | Receptionist | Update appointment status. |
| GET | `/Receptionist/PatientSearch` | Receptionist | Search for patients. |
| GET | `/Receptionist/LiveQueue` | Receptionist | Display the live appointment queue. |
| POST | `/Receptionist/UpdateQueueStatus` | Receptionist | Update live queue appointment status. |

---

### Doctor

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/Doctor/Dashboard` | Doctor | Display the doctor's dashboard. |
| GET | `/Doctor/Schedule` | Doctor | Display the doctor's schedule. |
| GET | `/Doctor/Notifications` | Doctor | Display doctor notifications. |
| POST | `/Doctor/MarkNotificationAsRead` | Doctor | Mark one notification as read. |
| POST | `/Doctor/MarkAllNotificationsAsRead` | Doctor | Mark all doctor notifications as read. |
| GET | `/Doctor/Profile` | Doctor | Display the doctor's profile. |
| GET | `/Doctor/EditProfile` | Doctor | Display the edit profile form. |
| POST | `/Doctor/EditProfile` | Doctor | Update doctor profile. |
| GET | `/Doctor/Appointments` | Doctor | Display the doctor's appointments. |
| GET | `/Doctor/AppointmentDetails/{id}` | Doctor | Display appointment details. |
| GET | `/Doctor/UpdateStatus/{id}` | Doctor | Display the appointment status update form. |
| POST | `/Doctor/UpdateStatus/{id?}` | Doctor | Update appointment status. |
| GET | `/Doctor/PatientHistory/{patientId}` | Doctor | Display a patient's medical history. |
| GET | `/Doctor/FollowUpRequest/{appointmentId}` | Doctor | Display follow-up appointment request form. |
| POST | `/Doctor/CreateFollowUpRequest` | Doctor | Create a follow-up appointment request. |
| GET | `/Doctor/Prescriptions` | Doctor | Display doctor prescriptions. |
| GET | `/Doctor/CreateVisitRecord/{appointmentId}` | Doctor | Display the create visit record form. |
| POST | `/Doctor/CreateVisitRecord/{appointmentId?}` | Doctor | Create a visit record for an appointment. |
| GET | `/Doctor/EditVisitRecord/{appointmentId}` | Doctor | Display the edit visit record form. |
| POST | `/Doctor/EditVisitRecord/{appointmentId?}` | Doctor | Update an existing visit record. |

---

### Clinic Manager

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/ClinicManager/Dashboard` | Clinic Manager | Display the clinic manager dashboard. |
| GET | `/ClinicManager/Doctors` | Clinic Manager | List and filter doctors. |
| GET | `/ClinicManager/DoctorDetails/{id}` | Clinic Manager | Display doctor details. |
| GET | `/ClinicManager/CreateDoctor` | Clinic Manager | Display the create doctor form. |
| POST | `/ClinicManager/CreateDoctor` | Clinic Manager | Create a new doctor account and profile. |
| GET | `/ClinicManager/EditDoctor/{id}` | Clinic Manager | Display the edit doctor form. |
| POST | `/ClinicManager/EditDoctor` | Clinic Manager | Update doctor information. |
| GET | `/ClinicManager/ManageDoctorSchedule` | Clinic Manager | Display doctor schedule management. |
| GET | `/ClinicManager/CreateDoctorSchedule` | Clinic Manager | Display the create doctor schedule form. |
| POST | `/ClinicManager/CreateDoctorSchedule` | Clinic Manager | Create a doctor schedule. |
| GET | `/ClinicManager/EditDoctorSchedule/{id}` | Clinic Manager | Display the edit doctor schedule form. |
| POST | `/ClinicManager/EditDoctorSchedule` | Clinic Manager | Update doctor schedule. |
| POST | `/ClinicManager/DeleteDoctorSchedule/{id}` | Clinic Manager | Delete doctor schedule. |
| GET | `/ClinicManager/ManageDoctorLeaves` | Clinic Manager | Display doctor leave management. |
| GET | `/ClinicManager/CreateDoctorLeave` | Clinic Manager | Display the create doctor leave form. |
| POST | `/ClinicManager/CreateDoctorLeave` | Clinic Manager | Create doctor leave. |
| GET | `/ClinicManager/EditDoctorLeave/{id}` | Clinic Manager | Display the edit doctor leave form. |
| POST | `/ClinicManager/EditDoctorLeave` | Clinic Manager | Update doctor leave. |
| POST | `/ClinicManager/DeleteDoctorLeave/{id}` | Clinic Manager | Delete doctor leave. |
| GET | `/ClinicManager/AppointmentImpact` | Clinic Manager | Display appointments affected by doctor schedule or leave changes. |
| POST | `/ClinicManager/CancelImpactedAppointment` | Clinic Manager | Cancel an impacted appointment. |
| POST | `/ClinicManager/RescheduleImpactedAppointment` | Clinic Manager | Reschedule an impacted appointment. |
| GET | `/ClinicManager/Appointments` | Clinic Manager | Display and filter clinic appointments. |
| GET | `/ClinicManager/AppointmentDetails/{id}` | Clinic Manager | Display appointment details. |
| GET | `/ClinicManager/UpdateAppointmentStatus/{id}` | Clinic Manager | Display the appointment status update form. |
| POST | `/ClinicManager/UpdateAppointmentStatus` | Clinic Manager | Update appointment status. |
| GET | `/ClinicManager/ManageDoctorSpecializations` | Clinic Manager | Display doctor specialization management. |
| POST | `/ClinicManager/ManageDoctorSpecializations` | Clinic Manager | Update doctor specializations. |
| GET | `/ClinicManager/Reports` | Clinic Manager | Display clinic reports dashboard. |
| GET | `/ClinicManager/CreateAnnouncement` | Clinic Manager | Display the clinic announcement form. |
| POST | `/ClinicManager/CreateAnnouncement` | Clinic Manager | Create and send a clinic announcement. |
| GET | `/ClinicManager/Notifications` | Clinic Manager | Display clinic manager notifications. |
| POST | `/ClinicManager/MarkNotificationAsRead/{id}` | Clinic Manager | Mark one notification as read. |
| POST | `/ClinicManager/MarkAllNotificationsAsRead` | Clinic Manager | Mark all notifications as read. |
| GET | `/ClinicManager/Profile` | Clinic Manager | Display clinic manager profile. |
| GET | `/ClinicManager/EditProfile` | Clinic Manager | Display the edit profile form. |
| POST | `/ClinicManager/EditProfile` | Clinic Manager | Update clinic manager profile. |
| GET | `/ClinicManager/UserAccounts` | Clinic Manager | Display user account activation and deactivation page. |
| POST | `/ClinicManager/ToggleUserStatus` | Clinic Manager | Activate or deactivate a user account. |

---

### Dashboard

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/Dashboard/Index` | Authenticated | Redirect the user to the correct dashboard based on role. |

---

### SignalR Hub

| Method | Route | Access | Description |
|---|---|---|---|
| HUB | `/hubs/appointment` | Authenticated | Real-time appointment update hub. |

---

## Web API Endpoints

### Authentication API

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/Auth/register` | None | Register a new user account. |
| POST | `/api/Auth/login` | None | Login and return a JWT token. |

---

### Appointment API

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/Appointment/lookup` | None | Public appointment lookup by CPR and optional patient reference. |
| GET | `/api/Appointment` | JWT + Receptionist / Clinic Manager | Get all appointments. |
| GET | `/api/Appointment/my` | JWT + Patient | Get the current patient's appointments. |
| PUT | `/api/Appointment/{id}/status` | JWT + Doctor / Receptionist / Clinic Manager | Update appointment status. |

---

### Doctor API

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/Doctor` | JWT | Get all doctors. |
| GET | `/api/Doctor/{id}` | JWT | Get doctor details by ID. |
| POST | `/api/Doctor` | JWT + Clinic Manager | Create a new doctor. |
| GET | `/api/Doctor/{id}/availability` | JWT | Get available time slots for a doctor. |

---

### Patient API

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/Patient` | JWT + Patient / Receptionist / Clinic Manager | Create a patient record. |
| GET | `/api/Patient` | JWT + Receptionist / Clinic Manager | Get all patients. |
| GET | `/api/Patient/{id}` | JWT + Doctor / Receptionist / Clinic Manager | Get patient details by ID. |
| GET | `/api/Patient/my` | JWT + Patient | Get the current patient's profile. |
| PUT | `/api/Patient/{id}` | JWT + Patient / Receptionist / Clinic Manager | Update patient information. |
| GET | `/api/Patient/{id}/history` | JWT + Doctor / Receptionist / Clinic Manager | Get a patient's visit history. |

---

### Reports API

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/Report/appointment-stats` | JWT + Clinic Manager | Get appointment statistics by status and date range. |
| GET | `/api/Report/doctor-workload` | JWT + Clinic Manager | Get doctor workload and utilization report. |
| GET | `/api/Report/specialization-stats` | JWT + Clinic Manager | Get appointment statistics by specialization. |
| GET | `/api/Report/daily-summary` | JWT + Clinic Manager | Get daily clinic summary. |

---

### API SignalR Hub

| Method | Route | Auth | Description |
|---|---|---|---|
| HUB | `/hubs/appointment` | JWT / Authorized users | Real-time appointment update hub. |

---


## Test Credentials

### Clinic Manager

| Email | Password |
|---|---|
| hussain@gentlecare.com | Hussain@123 |

### Receptionist

| Email | Password |
|---|---|
| sayedjaffar@gentlecare.com | Sayed@123 |

### Doctors

| Doctor Name | Email | Password |
|---|---|---|
| Fatema Mohamed | fatema@gentlecare.com | Fatema@123 |
| Hassan Ali | hassan@gentlecare.com | Hassan@123 |
| Ali Mohamed | ali@gentlecare.com | Ali@1234 |
| Jawad Ali | jawad@gentlecare.com | Jawad@123 |
| Masooma Ridha | masooma@gentlecare.com | Masooma@123 |
| Abbas Ali | abbas@gentlecare.com | Abbas@123 |

### Patients

| Patient Name | Email | Password | CPR Number | Patient Reference Number |
|---|---|---|---|---|
| Sayed Hassan | sayedhassan@gmail.com | Sayed@123 | 900101001 | PAT-1001 |
| Mohamed Baqer | mohamed@gmail.com | Mohamed@123 | 980312002 | PAT-1002 |
| Zahraa Ahmed | zahraa@gmail.com | Zahraa@123 | 010705003 | PAT-1003 |
| Mohsen Ali | mohsen@gmail.com | Mohsen@123 | 951122004 | PAT-1004 |
| Zainab Abbas | zainab@gmail.com | Maryam@123 | 070218005 | PAT-1005 |
| Sajjad Ali | sajjad@gmail.com | Sajjad@123 | 890909006 | PAT-1006 |
| Mahdi Mohamed | mahdi@gmail.com | Mahdi@123 | 040415007 | PAT-1007 |
| Hadi Ali | hadi@gmail.com | Hadi@123 | 001230008 | PAT-1008 |

### Public Lookup Test Data

Patients can use the public lookup page with their CPR number and patient reference number.

| Patient Name | CPR Number | Patient Reference Number |
|---|---|---|
| Sayed Hassan | 900101001 | PAT-1001 |
| Mohamed Baqer | 980312002 | PAT-1002 |
| Zahraa Ahmed | 010705003 | PAT-1003 |
| Mohsen Ali | 951122004 | PAT-1004 |
| Zainab Abbas | 070218005 | PAT-1005 |
| Sajjad Ali | 890909006 | PAT-1006 |
| Mahdi Mohamed | 040415007 | PAT-1007 |
| Hadi Ali | 001230008 | PAT-1008 |

---

## Technologies Used

| Technology            | Purpose                                                               |
| --------------------- | --------------------------------------------------------------------- |
| ASP.NET Core MVC      | Main user-facing web application.                                     |
| ASP.NET Core Web API  | Backend API and shared data layer.                                    |
| Entity Framework Core | Database access and object-relational mapping.                        |
| ASP.NET Core Identity | Authentication, roles, and user account management.                   |
| SQL Server            | Relational database.                                                  |
| SignalR               | Real-time appointment and queue updates.                              |
| JWT Authentication    | Secures API endpoints.                                                |
| HttpClient            | Used by MVC public lookup and Reporting App to consume API endpoints. |
| Bootstrap             | Responsive user interface styling.                                    |
| Azure App Service     | Deployment for MVC, API, and Reporting App.                           |
| Azure SQL Database    | Cloud-hosted database.                                                |

---

## Security Features

* Role-based authorization protects pages and actions.
* Clinic Manager, Doctor, Patient, and Receptionist users only access pages relevant to their roles.
* Passwords are handled through ASP.NET Core Identity.
* Inactive users are blocked from logging in.
* Public lookup does not expose full patient account access.
* Reporting application is read-only.
* Anti-forgery tokens are used on form submissions.
* API endpoints are secured using JWT where required.

---

## Appointment Workflow

Appointments follow this workflow:

```text
Requested -> Confirmed -> Checked-In -> In Progress -> Completed
```

Appointments may also end as:

```text
Cancelled
Missed
```

The system validates appointment status transitions to prevent invalid workflow changes.

---

## Real-Time Functionality

GentleCare uses SignalR to support real-time updates.

Examples include:

* Live appointment queue updates.
* Appointment status changes.
* Manager status updates broadcast to connected views.
* Receptionist and waiting-room style updates.

---

## Deployment

The system has been deployed on Microsoft Azure, with the MVC Application, Web API, and Reporting Application hosted on Azure App Service and the database hosted on Azure SQL Database.

---

## How to Run Locally

1. Clone the repository.
2. Open `GentleCare.sln` in Visual Studio.
3. Set the correct connection string in `appsettings.json`.
4. Run database migrations or restore the provided SQL scripts.
5. Start the WebAPI project.
6. Start the MVCApp project.
7. Login using the seeded demo accounts.
