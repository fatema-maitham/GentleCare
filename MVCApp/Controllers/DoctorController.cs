using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCApp.ViewModels.Doctor;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    // Handles all MVC pages for the Doctor role.
    // Doctors can view their own appointments, update appointment progress,
    // create visit records, add prescriptions, and view their notifications.
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Centralized status names to avoid spelling mistakes in workflow checks.
        private static class StatusNames
        {
            public const string Requested = "Requested";
            public const string Confirmed = "Confirmed";
            public const string CheckedIn = "CheckedIn";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string Missed = "Missed";
        }

        public DoctorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? selectedDate = null)
        {
            ViewData["Title"] = "Doctor Dashboard";

            var doctor = await GetCurrentDoctorWithUserAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var chosenDate = selectedDate?.Date ?? DateTime.Today;

            var firstDayOfMonth = new DateTime(chosenDate.Year, chosenDate.Month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var monthAppointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.AppointmentDate >= firstDayOfMonth &&
                    a.AppointmentDate < firstDayOfNextMonth)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var selectedDayAppointments = monthAppointments
                .Where(a => a.AppointmentDate.Date == chosenDate.Date)
                .OrderBy(a => a.StartTime)
                .ToList();

            var upcomingAppointmentsCount = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.AppointmentDate.Date >= DateTime.Today &&
                    a.Status.Name != "Completed" &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Missed")
                .CountAsync();

            var unreadNotificationsCount = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == doctor.UserId && !n.IsRead)
                .CountAsync();

            var totalPatientsSeen = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.Status.Name == "Completed")
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            var model = new DoctorDashboardViewModel
            {
                DoctorFullName = doctor.User.FullName,
                SelectedDate = chosenDate,
                TotalAppointmentsForSelectedDate = selectedDayAppointments.Count,
                ConfirmedAppointmentsForSelectedDate = selectedDayAppointments.Count(a => a.Status.Name == "Confirmed"),
                CheckedInAppointmentsForSelectedDate = selectedDayAppointments.Count(a => a.Status.Name == "CheckedIn"),
                InProgressAppointmentsForSelectedDate = selectedDayAppointments.Count(a => a.Status.Name == "InProgress"),
                CompletedAppointmentsForSelectedDate = selectedDayAppointments.Count(a => a.Status.Name == "Completed"),
                UpcomingAppointmentsCount = upcomingAppointmentsCount,
                UnreadNotificationsCount = unreadNotificationsCount,
                TotalPatientsSeen = totalPatientsSeen,
                CalendarDays = BuildDoctorCalendarDays(chosenDate, monthAppointments),
                SelectedDayAppointments = selectedDayAppointments.Select(a => new DoctorDashboardAppointmentItemViewModel
                {
                    AppointmentId = a.Id,
                    PatientFullName = a.Patient.User.FullName,
                    PatientReferenceNumber = a.Patient.ReferenceNumber,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StatusName = FormatDoctorCalendarStatus(a.Status.Name),
                    Notes = a.Notes
                }).ToList()
            };

            return View(model);
        }


        private List<DoctorDashboardCalendarDayViewModel> BuildDoctorCalendarDays(
    DateTime selectedDate,
    List<Appointment> monthAppointments)
        {
            var firstDayOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var startOffset = (int)firstDayOfMonth.DayOfWeek; // Sunday = 0
            var endOffset = 6 - (int)lastDayOfMonth.DayOfWeek;

            var calendarStart = firstDayOfMonth.AddDays(-startOffset);
            var calendarEnd = lastDayOfMonth.AddDays(endOffset);

            var appointmentCounts = monthAppointments
                .GroupBy(a => a.AppointmentDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var days = new List<DoctorDashboardCalendarDayViewModel>();

            for (var date = calendarStart; date <= calendarEnd; date = date.AddDays(1))
            {
                appointmentCounts.TryGetValue(date.Date, out var count);

                days.Add(new DoctorDashboardCalendarDayViewModel
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = date.Month == selectedDate.Month,
                    IsSelected = date.Date == selectedDate.Date,
                    HasAppointments = count > 0,
                    AppointmentCount = count
                });
            }

            return days;
        }

        private string FormatDoctorCalendarStatus(string statusName)
        {
            return statusName switch
            {
                "CheckedIn" => "Checked In",
                "InProgress" => "In Progress",
                _ => statusName
            };
        }

        // Lists only the logged-in doctor's appointments.
        // Optional filters allow the doctor to search by patient, status, or date.
        [HttpGet]
        public async Task<IActionResult> Appointments(string? searchTerm = null, string? status = null, DateTime? date = null)
        {
            ViewData["Title"] = "My Appointments";

            var doctor = await GetCurrentDoctorWithUserAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            searchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
            status = string.IsNullOrWhiteSpace(status) ? null : status.Trim();

            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Status)
                .Where(a => a.DoctorId == doctor.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a =>
                    a.Patient.User.FullName.Contains(searchTerm) ||
                    a.Patient.CPRNumber.Contains(searchTerm) ||
                    a.Patient.ReferenceNumber.Contains(searchTerm) ||
                    (a.Notes != null && a.Notes.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status.Name == status);
            }

            if (date.HasValue)
            {
                var selectedDate = date.Value.Date;
                query = query.Where(a => a.AppointmentDate.Date == selectedDate);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var statusOptions = await _context.AppointmentStatuses
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = FormatStatusName(s.Name),
                    Selected = s.Name == status
                })
                .ToListAsync();

            var model = new DoctorAppointmentListViewModel
            {
                DoctorFullName = doctor.User.FullName,
                SearchTerm = searchTerm,
                SelectedStatus = status,
                SelectedDate = date,
                StatusOptions = statusOptions,
                Appointments = appointments.Select(a => new DoctorAppointmentListItemViewModel
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    PatientId = a.PatientId,
                    PatientFullName = a.Patient.User.FullName,
                    PatientReferenceNumber = a.Patient.ReferenceNumber,
                    StatusName = FormatStatusName(a.Status.Name),
                    Notes = a.Notes
                }).ToList()
            };

            return View(model);
        }



        // Shows full appointment details, including patient information,
        // visit record, prescriptions, and allowed next status actions.
        [HttpGet]
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            ViewData["Title"] = "Appointment Details";

            var appointment = await GetDoctorAppointmentAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var allowedNextStatuses = BuildAllowedStatusSelectList(appointment.Status.Name);

            var model = new DoctorAppointmentDetailsViewModel
            {
                AppointmentId = appointment.Id,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                StatusName = FormatStatusName(appointment.Status.Name),
                Notes = appointment.Notes,
                CancellationReason = appointment.CancellationReason,

                PatientId = appointment.PatientId,
                PatientFullName = appointment.Patient.User.FullName,
                PatientReferenceNumber = appointment.Patient.ReferenceNumber,
                PatientCprNumber = appointment.Patient.CPRNumber,

                DoctorNotes = appointment.VisitRecord?.DoctorNotes,
                Diagnosis = appointment.VisitRecord?.Diagnosis,
                Treatment = appointment.VisitRecord?.Treatment,

                HasVisitRecord = appointment.VisitRecord != null,
                CanCreateVisitRecord = appointment.VisitRecord == null && CanCreateVisitRecord(appointment.Status.Name),
                CanEditVisitRecord = appointment.VisitRecord != null,
                CanUpdateStatus = allowedNextStatuses.Any(),
                AvailableNextStatuses = allowedNextStatuses,

                Prescriptions = appointment.VisitRecord?.Prescriptions
                    .Select(p => new PrescriptionInputViewModel
                    {
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        Frequency = p.Frequency,
                        DurationDays = p.DurationDays,
                        Instructions = p.Instructions
                    })
                    .ToList()
                    ?? new List<PrescriptionInputViewModel>()
            };

            return View(model);
        }

        // Displays the valid next statuses for this appointment based on
        // the clinic appointment lifecycle rules.
        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            ViewData["Title"] = "Update Appointment Status";

            var appointment = await GetDoctorAppointmentAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var availableStatuses = BuildAllowedStatusSelectList(appointment.Status.Name);
            if (!availableStatuses.Any())
            {
                TempData["Error"] = "This appointment cannot be updated by the doctor at its current status.";
                return RedirectToAction(nameof(AppointmentDetails), new { id });
            }

            var model = new UpdateAppointmentStatusViewModel
            {
                AppointmentId = appointment.Id,
                CurrentStatusName = FormatStatusName(appointment.Status.Name),
                NewStatusName = string.Empty,
                AvailableStatuses = availableStatuses
            };

            return View(model);
        }

        // Updates the appointment status after validating that the transition
        // is allowed for the doctor workflow.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateAppointmentStatusViewModel model)
        {
            ViewData["Title"] = "Update Appointment Status";

            var appointment = await GetDoctorAppointmentAsync(model.AppointmentId);
            if (appointment == null)
            {
                return NotFound();
            }

            var allowedStatuses = GetAllowedNextStatuses(appointment.Status.Name);

            if (string.IsNullOrWhiteSpace(model.NewStatusName))
            {
                ModelState.AddModelError(nameof(model.NewStatusName), "Please select a new status.");
            }
            else if (!allowedStatuses.Contains(model.NewStatusName))
            {
                ModelState.AddModelError(nameof(model.NewStatusName), "Invalid status transition for doctor workflow.");
            }

            // A doctor should not mark an appointment as missed before its scheduled time has passed.
            if (model.NewStatusName == StatusNames.Missed && !CanMarkAsMissed(appointment))
            {
                ModelState.AddModelError(nameof(model.NewStatusName), "The appointment can only be marked as missed after its scheduled time has passed.");
            }

            if (!ModelState.IsValid)
            {
                model.CurrentStatusName = FormatStatusName(appointment.Status.Name);
                model.AvailableStatuses = BuildAllowedStatusSelectList(appointment.Status.Name);
                return View(model);
            }

            var newStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == model.NewStatusName);

            if (newStatus == null)
            {
                ModelState.AddModelError(nameof(model.NewStatusName), "Selected status does not exist.");
                model.CurrentStatusName = FormatStatusName(appointment.Status.Name);
                model.AvailableStatuses = BuildAllowedStatusSelectList(appointment.Status.Name);
                return View(model);
            }

            var oldStatusName = appointment.Status.Name;

            appointment.StatusId = newStatus.Id;
            appointment.UpdatedAt = DateTime.UtcNow;

            await CreateAppointmentStatusNotificationAsync(
                appointment,
                oldStatusName,
                newStatus.Name);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment status updated successfully.";
            return RedirectToAction(nameof(AppointmentDetails), new { id = appointment.Id });
        }

        // Shows the visit history for a patient only if the current doctor
        // has treated or has an appointment with that patient.
        [HttpGet]
        public async Task<IActionResult> PatientHistory(int patientId)
        {
            ViewData["Title"] = "Patient History";

            var doctor = await GetCurrentDoctorWithUserAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var patient = await _context.Patients
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return NotFound();
            }

            // Security check: doctors can only view history for their own patients.
            var hasRelationship = await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.DoctorId == doctor.Id && a.PatientId == patientId);

            if (!hasRelationship)
            {
                return Forbid();
            }

            var visits = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v.Prescriptions)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.PatientId == patientId &&
                    a.VisitRecord != null)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .ToListAsync();

            var model = new DoctorPatientHistoryViewModel
            {
                PatientId = patient.Id,
                PatientFullName = patient.User.FullName,
                CPRNumber = patient.CPRNumber,
                ReferenceNumber = patient.ReferenceNumber,
                DateOfBirth = patient.DateOfBirth,
                BloodType = patient.BloodType,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                Visits = visits.Select(a => new DoctorPatientVisitItemViewModel
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StatusName = FormatStatusName(a.Status.Name),
                    DoctorNotes = a.VisitRecord!.DoctorNotes,
                    Diagnosis = a.VisitRecord.Diagnosis,
                    Treatment = a.VisitRecord.Treatment,
                    PrescriptionCount = a.VisitRecord.Prescriptions.Count
                }).ToList()
            };

            return View(model);
        }

        // Opens the visit record form for an appointment that is in progress
        // or completed and does not already have a visit record.
        [HttpGet]
        public async Task<IActionResult> CreateVisitRecord(int appointmentId)
        {
            ViewData["Title"] = "Create Visit Record";

            var appointment = await GetDoctorAppointmentAsync(appointmentId);
            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.VisitRecord != null)
            {
                TempData["Error"] = "A visit record already exists for this appointment.";
                return RedirectToAction(nameof(AppointmentDetails), new { id = appointmentId });
            }

            if (!CanCreateVisitRecord(appointment.Status.Name))
            {
                TempData["Error"] = "Visit record can only be created when the appointment is in progress or completed.";
                return RedirectToAction(nameof(AppointmentDetails), new { id = appointmentId });
            }

            var model = new CreateVisitRecordViewModel
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                PatientFullName = appointment.Patient.User.FullName,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Prescriptions = new List<PrescriptionInputViewModel>
                {
                    new PrescriptionInputViewModel()
                }
            };

            return View(model);
        }

        // Saves the doctor's notes, diagnosis, treatment, and prescriptions.
        // The appointment is automatically marked as completed after the visit record is saved.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVisitRecord(CreateVisitRecordViewModel model)
        {
            ViewData["Title"] = "Create Visit Record";

            var appointment = await GetDoctorAppointmentAsync(model.AppointmentId);
            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.VisitRecord != null)
            {
                TempData["Error"] = "A visit record already exists for this appointment.";
                return RedirectToAction(nameof(AppointmentDetails), new { id = model.AppointmentId });
            }

            if (!CanCreateVisitRecord(appointment.Status.Name))
            {
                TempData["Error"] = "Visit record can only be created when the appointment is in progress or completed.";
                return RedirectToAction(nameof(AppointmentDetails), new { id = model.AppointmentId });
            }

            ValidateVisitRecordInput(model.DoctorNotes, model.Diagnosis);

            var cleanedPrescriptions = NormalizePrescriptionInputs(model.Prescriptions);
            ValidatePrescriptionInputs(cleanedPrescriptions);

            if (!ModelState.IsValid)
            {
                PopulateVisitRecordCreateMeta(model, appointment);
                model.Prescriptions = cleanedPrescriptions.Any()
                    ? cleanedPrescriptions
                    : new List<PrescriptionInputViewModel> { new PrescriptionInputViewModel() };

                return View(model);
            }

            // Transaction keeps the visit record, prescriptions, status update,
            // and notification saved together as one complete operation.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var visitRecord = new VisitRecord
                {
                    AppointmentId = appointment.Id,
                    DoctorNotes = model.DoctorNotes.Trim(),
                    Diagnosis = model.Diagnosis.Trim(),
                    Treatment = string.IsNullOrWhiteSpace(model.Treatment) ? null : model.Treatment.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.VisitRecords.Add(visitRecord);
                await _context.SaveChangesAsync();

                if (cleanedPrescriptions.Any())
                {
                    var prescriptions = cleanedPrescriptions.Select(p => new Prescription
                    {
                        VisitRecordId = visitRecord.Id,
                        MedicationName = p.MedicationName.Trim(),
                        Dosage = p.Dosage.Trim(),
                        Frequency = p.Frequency.Trim(),
                        DurationDays = p.DurationDays,
                        Instructions = string.IsNullOrWhiteSpace(p.Instructions) ? null : p.Instructions.Trim(),
                        CreatedAt = DateTime.UtcNow
                    });

                    await _context.Prescriptions.AddRangeAsync(prescriptions);
                }

                if (appointment.Status.Name != StatusNames.Completed)
                {
                    var completedStatus = await _context.AppointmentStatuses
                        .FirstAsync(s => s.Name == StatusNames.Completed);

                    appointment.StatusId = completedStatus.Id;
                    appointment.UpdatedAt = DateTime.UtcNow;
                }

                await CreatePrescriptionNotificationIfNeededAsync(appointment, cleanedPrescriptions.Count);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Visit record created successfully.";
                return RedirectToAction(nameof(AppointmentDetails), new { id = appointment.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "An error occurred while creating the visit record.");

                PopulateVisitRecordCreateMeta(model, appointment);
                model.Prescriptions = cleanedPrescriptions.Any()
                    ? cleanedPrescriptions
                    : new List<PrescriptionInputViewModel> { new PrescriptionInputViewModel() };

                return View(model);
            }
        }

        // Opens an existing visit record so the doctor can update notes,
        // diagnosis, treatment, or prescriptions.
        [HttpGet]
        public async Task<IActionResult> EditVisitRecord(int appointmentId)
        {
            ViewData["Title"] = "Edit Visit Record";

            var appointment = await GetDoctorAppointmentAsync(appointmentId);
            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.VisitRecord == null)
            {
                TempData["Error"] = "No visit record exists for this appointment.";
                return RedirectToAction(nameof(AppointmentDetails), new { id = appointmentId });
            }

            var model = new EditVisitRecordViewModel
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                PatientFullName = appointment.Patient.User.FullName,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                DoctorNotes = appointment.VisitRecord.DoctorNotes,
                Diagnosis = appointment.VisitRecord.Diagnosis,
                Treatment = appointment.VisitRecord.Treatment,
                Prescriptions = appointment.VisitRecord.Prescriptions.Any()
                    ? appointment.VisitRecord.Prescriptions.Select(p => new PrescriptionInputViewModel
                    {
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        Frequency = p.Frequency,
                        DurationDays = p.DurationDays,
                        Instructions = p.Instructions
                    }).ToList()
                    : new List<PrescriptionInputViewModel> { new PrescriptionInputViewModel() }
            };

            return View(model);
        }

        // Updates the existing visit record and replaces the old prescription list
        // with the latest prescription details entered by the doctor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVisitRecord(EditVisitRecordViewModel model)
        {
            ViewData["Title"] = "Edit Visit Record";

            var appointment = await GetDoctorAppointmentAsync(model.AppointmentId);
            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.VisitRecord == null)
            {
                TempData["Error"] = "No visit record exists for this appointment.";
                return RedirectToAction(nameof(AppointmentDetails), new { id = model.AppointmentId });
            }

            ValidateVisitRecordInput(model.DoctorNotes, model.Diagnosis);

            var cleanedPrescriptions = NormalizePrescriptionInputs(model.Prescriptions);
            ValidatePrescriptionInputs(cleanedPrescriptions);

            if (!ModelState.IsValid)
            {
                PopulateVisitRecordEditMeta(model, appointment);
                model.Prescriptions = cleanedPrescriptions.Any()
                    ? cleanedPrescriptions
                    : new List<PrescriptionInputViewModel> { new PrescriptionInputViewModel() };

                return View(model);
            }

            // Transaction prevents partial updates if prescription replacement fails.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                appointment.VisitRecord.DoctorNotes = model.DoctorNotes.Trim();
                appointment.VisitRecord.Diagnosis = model.Diagnosis.Trim();
                appointment.VisitRecord.Treatment = string.IsNullOrWhiteSpace(model.Treatment)
                    ? null
                    : model.Treatment.Trim();

                _context.Prescriptions.RemoveRange(appointment.VisitRecord.Prescriptions);

                if (cleanedPrescriptions.Any())
                {
                    var newPrescriptions = cleanedPrescriptions.Select(p => new Prescription
                    {
                        VisitRecordId = appointment.VisitRecord.Id,
                        MedicationName = p.MedicationName.Trim(),
                        Dosage = p.Dosage.Trim(),
                        Frequency = p.Frequency.Trim(),
                        DurationDays = p.DurationDays,
                        Instructions = string.IsNullOrWhiteSpace(p.Instructions) ? null : p.Instructions.Trim(),
                        CreatedAt = DateTime.UtcNow
                    });

                    await _context.Prescriptions.AddRangeAsync(newPrescriptions);
                }

                appointment.UpdatedAt = DateTime.UtcNow;

                await CreatePrescriptionNotificationIfNeededAsync(appointment, cleanedPrescriptions.Count);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Visit record updated successfully.";
                return RedirectToAction(nameof(AppointmentDetails), new { id = appointment.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "An error occurred while updating the visit record.");

                PopulateVisitRecordEditMeta(model, appointment);
                model.Prescriptions = cleanedPrescriptions.Any()
                    ? cleanedPrescriptions
                    : new List<PrescriptionInputViewModel> { new PrescriptionInputViewModel() };

                return View(model);
            }
        }

        // Displays prescriptions recorded by the logged-in doctor.
        [HttpGet]
        public async Task<IActionResult> Prescriptions()
        {
            ViewData["Title"] = "My Prescriptions";

            var doctor = await GetCurrentDoctorWithUserAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var prescriptions = await _context.Prescriptions
                .AsNoTracking()
                .Include(p => p.VisitRecord)
                    .ThenInclude(v => v.Appointment)
                        .ThenInclude(a => a.Patient)
                            .ThenInclude(p => p.User)
                .Where(p => p.VisitRecord.Appointment.DoctorId == doctor.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new DoctorPrescriptionViewModel
                {
                    PrescriptionId = p.Id,
                    VisitRecordId = p.VisitRecordId,
                    AppointmentId = p.VisitRecord.AppointmentId,
                    PatientId = p.VisitRecord.Appointment.PatientId,
                    PatientFullName = p.VisitRecord.Appointment.Patient.User.FullName,
                    PatientReferenceNumber = p.VisitRecord.Appointment.Patient.ReferenceNumber,
                    AppointmentDate = p.VisitRecord.Appointment.AppointmentDate,
                    MedicationName = p.MedicationName,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    DurationDays = p.DurationDays,
                    Instructions = p.Instructions,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return View(prescriptions);
        }

        // Displays all notifications for the logged-in doctor.
        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            ViewData["Title"] = "My Notifications";

            var doctor = await GetCurrentDoctorWithUserAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == doctor.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var model = new DoctorNotificationsViewModel
            {
                UnreadCount = notifications.Count(n => !n.IsRead),
                Notifications = notifications.Select(n => new DoctorNotificationViewModel
                {
                    NotificationId = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    RelatedEntityId = n.RelatedEntityId,
                    RelatedEntityType = n.RelatedEntityType
                }).ToList()
            };

            return View(model);
        }

        // Marks one doctor notification as read.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var doctor = await GetCurrentDoctorWithUserAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == doctor.UserId);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Notification marked as read.";
            return RedirectToAction(nameof(Notifications));
        }

        // Marks all unread doctor notifications as read.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var doctor = await GetCurrentDoctorWithUserAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == doctor.UserId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Notifications));
        }

        // Displays the doctor's profile and assigned specializations.
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            ViewData["Title"] = "My Profile";

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == _userManager.GetUserId(User));

            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var model = new DoctorProfileViewModel
            {
                DoctorId = doctor.Id,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                Specializations = doctor.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name)
                    .OrderBy(name => name)
                    .ToList()
            };

            return View(model);
        }

        // Displays the doctor's weekly schedule and approved leave periods.
        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            ViewData["Title"] = "My Schedule";

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.UserId == _userManager.GetUserId(User));

            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var model = new DoctorScheduleViewModel
            {
                DoctorFullName = doctor.User.FullName,
                Schedules = doctor.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .ToList(),
                Leaves = doctor.Leaves
                    .OrderByDescending(l => l.StartDate)
                    .ToList()
            };

            return View(model);
        }

        // Finds the Doctor record linked to the currently logged-in Identity user.
        private async Task<Doctor?> GetCurrentDoctorWithUserAsync()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.User.IsActive);
        }

        // Loads an appointment only if it belongs to the currently logged-in doctor.
        private async Task<Appointment?> GetDoctorAppointmentAsync(int appointmentId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v.Prescriptions)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    a.Doctor.UserId == userId);
        }

        // Defines the allowed appointment workflow transitions for doctors.
        private static List<string> GetAllowedNextStatuses(string currentStatusName)
        {
            return currentStatusName switch
            {
                StatusNames.Confirmed => new List<string>
                {
                    StatusNames.CheckedIn,
                    StatusNames.Missed
                },

                StatusNames.CheckedIn => new List<string>
                {
                    StatusNames.InProgress,
                    StatusNames.Missed
                },

                StatusNames.InProgress => new List<string>
                {
                    StatusNames.Completed
                },

                _ => new List<string>()
            };
        }

        private static List<SelectListItem> BuildAllowedStatusSelectList(string currentStatusName)
        {
            return GetAllowedNextStatuses(currentStatusName)
                .Select(status => new SelectListItem
                {
                    Value = status,
                    Text = FormatStatusName(status)
                })
                .ToList();
        }

        // Visit records should only be created once the consultation is in progress or completed.
        private static bool CanCreateVisitRecord(string currentStatusName)
        {
            return currentStatusName == StatusNames.InProgress ||
                   currentStatusName == StatusNames.Completed;
        }

        // Prevents marking future appointments as missed.
        private static bool CanMarkAsMissed(Appointment appointment)
        {
            var appointmentEndDateTime = appointment.AppointmentDate.Date
                .Add(appointment.EndTime.ToTimeSpan());

            return DateTime.Now >= appointmentEndDateTime;
        }

        private void ValidateVisitRecordInput(string? doctorNotes, string? diagnosis)
        {
            if (string.IsNullOrWhiteSpace(doctorNotes))
            {
                ModelState.AddModelError(nameof(CreateVisitRecordViewModel.DoctorNotes), "Doctor notes are required.");
            }

            if (string.IsNullOrWhiteSpace(diagnosis))
            {
                ModelState.AddModelError(nameof(CreateVisitRecordViewModel.Diagnosis), "Diagnosis is required.");
            }
        }

        private void ValidatePrescriptionInputs(List<PrescriptionInputViewModel> prescriptions)
        {
            for (var i = 0; i < prescriptions.Count; i++)
            {
                var item = prescriptions[i];

                if (string.IsNullOrWhiteSpace(item.MedicationName))
                {
                    ModelState.AddModelError($"Prescriptions[{i}].MedicationName", "Medication name is required.");
                }

                if (string.IsNullOrWhiteSpace(item.Dosage))
                {
                    ModelState.AddModelError($"Prescriptions[{i}].Dosage", "Dosage is required.");
                }

                if (string.IsNullOrWhiteSpace(item.Frequency))
                {
                    ModelState.AddModelError($"Prescriptions[{i}].Frequency", "Frequency is required.");
                }

                if (item.DurationDays <= 0)
                {
                    ModelState.AddModelError($"Prescriptions[{i}].DurationDays", "Duration must be greater than 0.");
                }
            }
        }

        // Removes empty prescription rows and trims user input before saving.
        private static List<PrescriptionInputViewModel> NormalizePrescriptionInputs(
            IEnumerable<PrescriptionInputViewModel>? prescriptions)
        {
            if (prescriptions == null)
            {
                return new List<PrescriptionInputViewModel>();
            }

            return prescriptions
                .Where(p =>
                    !string.IsNullOrWhiteSpace(p.MedicationName) ||
                    !string.IsNullOrWhiteSpace(p.Dosage) ||
                    !string.IsNullOrWhiteSpace(p.Frequency) ||
                    p.DurationDays > 0 ||
                    !string.IsNullOrWhiteSpace(p.Instructions))
                .Select(p => new PrescriptionInputViewModel
                {
                    MedicationName = p.MedicationName?.Trim() ?? string.Empty,
                    Dosage = p.Dosage?.Trim() ?? string.Empty,
                    Frequency = p.Frequency?.Trim() ?? string.Empty,
                    DurationDays = p.DurationDays,
                    Instructions = string.IsNullOrWhiteSpace(p.Instructions)
                        ? null
                        : p.Instructions.Trim()
                })
                .ToList();
        }

        private static void PopulateVisitRecordCreateMeta(
            CreateVisitRecordViewModel model,
            Appointment appointment)
        {
            model.PatientId = appointment.PatientId;
            model.PatientFullName = appointment.Patient.User.FullName;
            model.AppointmentDate = appointment.AppointmentDate;
            model.StartTime = appointment.StartTime;
            model.EndTime = appointment.EndTime;
        }

        private static void PopulateVisitRecordEditMeta(
            EditVisitRecordViewModel model,
            Appointment appointment)
        {
            model.PatientId = appointment.PatientId;
            model.PatientFullName = appointment.Patient.User.FullName;
            model.AppointmentDate = appointment.AppointmentDate;
            model.StartTime = appointment.StartTime;
            model.EndTime = appointment.EndTime;
        }

        // Creates a patient notification whenever the doctor changes appointment status.
        private async Task CreateAppointmentStatusNotificationAsync(
            Appointment appointment,
            string oldStatusName,
            string newStatusName)
        {
            var notificationTypeId = await GetNotificationTypeIdAsync("Appointment");

            _context.Notifications.Add(new Notification
            {
                UserId = appointment.Patient.UserId,
                NotificationTypeId = notificationTypeId,
                Title = "Appointment Status Updated",
                Message = $"Your appointment on {appointment.AppointmentDate:dd MMM yyyy} changed from {FormatStatusName(oldStatusName)} to {FormatStatusName(newStatusName)}.",
                RelatedEntityId = appointment.Id,
                RelatedEntityType = nameof(Appointment),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Notifies the patient when prescriptions are added or updated.
        private async Task CreatePrescriptionNotificationIfNeededAsync(
            Appointment appointment,
            int prescriptionCount)
        {
            if (prescriptionCount <= 0)
            {
                return;
            }

            var notificationTypeId = await GetNotificationTypeIdAsync("Prescription");

            _context.Notifications.Add(new Notification
            {
                UserId = appointment.Patient.UserId,
                NotificationTypeId = notificationTypeId,
                Title = "Prescription Updated",
                Message = $"A prescription has been recorded for your visit on {appointment.AppointmentDate:dd MMM yyyy}.",
                RelatedEntityId = appointment.Id,
                RelatedEntityType = nameof(Prescription),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task<int?> GetNotificationTypeIdAsync(string typeName)
        {
            return await _context.NotificationTypes
                .AsNoTracking()
                .Where(t => t.Name == typeName)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();
        }

        private static string FormatStatusName(string statusName)
        {
            return statusName switch
            {
                StatusNames.CheckedIn => "Checked In",
                StatusNames.InProgress => "In Progress",
                _ => statusName
            };
        }
    }
}