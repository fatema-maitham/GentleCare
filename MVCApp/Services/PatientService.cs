using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Patient;
using System.Security.Claims;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Services
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IClinicNotificationService _notificationService;

        public PatientService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            IClinicNotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _notificationService = notificationService;
        }

        public async Task<PatientDashboardViewModel?> GetDashboardAsync(ClaimsPrincipal userPrincipal)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return null;
            }

            var today = DateTime.Today;

            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Where(a => a.PatientId == patient.Id)
                .ToListAsync();

            var nextAppointment = appointments
                .Where(a =>
                    a.AppointmentDate.Date >= today &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Missed" &&
                    a.Status.Name != "Completed")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new PatientAppointmentsViewModel
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString("HH:mm"),
                    EndTime = a.EndTime.ToString("HH:mm"),
                    DoctorName = a.Doctor.User.FullName,
                    Status = a.Status.Name,
                    Notes = a.Notes
                })
                .FirstOrDefault();

            var latestVisit = await _context.VisitRecords
                .Include(v => v.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .Include(v => v.Prescriptions)
                .Where(v => v.Appointment.PatientId == patient.Id)
                .OrderByDescending(v => v.Appointment.AppointmentDate)
                .Select(v => new PatientHistoryViewModel
                {
                    AppointmentId = v.AppointmentId,
                    AppointmentDate = v.Appointment.AppointmentDate,
                    DoctorName = v.Appointment.Doctor.User.FullName,
                    Diagnosis = v.Diagnosis,
                    Treatment = v.Treatment,
                    DoctorNotes = v.DoctorNotes,
                    Prescriptions = v.Prescriptions.Select(p => new PrescriptionItemViewModel
                    {
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        Frequency = p.Frequency,
                        DurationDays = p.DurationDays,
                        Instructions = p.Instructions
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            var unreadNotificationsCount = await _context.Notifications
                .CountAsync(n => n.UserId == patient.UserId && !n.IsRead);

            return new PatientDashboardViewModel
            {
                FullName = patient.User.FullName,
                TotalAppointmentsCount = appointments.Count,
                UpcomingAppointmentsCount = appointments.Count(a =>
                    a.AppointmentDate.Date >= today &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Missed" &&
                    a.Status.Name != "Completed"),
                CompletedVisitsCount = appointments.Count(a => a.Status.Name == "Completed"),
                UnreadNotificationsCount = unreadNotificationsCount,
                NextAppointment = nextAppointment,
                LatestVisit = latestVisit
            };
        }

        public async Task<PatientProfileViewModel?> GetProfileAsync(ClaimsPrincipal userPrincipal)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return null;
            }

            return new PatientProfileViewModel
            {
                FullName = patient.User.FullName,
                Email = patient.User.Email ?? string.Empty,
                CPRNumber = patient.CPRNumber,
                ReferenceNumber = patient.ReferenceNumber,
                DateOfBirth = patient.DateOfBirth,
                BloodType = patient.BloodType,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                ProfilePicture = patient.User.ProfilePicture
            };
        }

        public async Task<PatientEditProfileViewModel?> GetEditProfileAsync(ClaimsPrincipal userPrincipal)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return null;
            }

            return new PatientEditProfileViewModel
            {
                FullName = patient.User.FullName,
                Email = patient.User.Email ?? string.Empty,
                CPRNumber = patient.CPRNumber,
                ReferenceNumber = patient.ReferenceNumber,
                DateOfBirth = patient.DateOfBirth,
                BloodType = patient.BloodType,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                CurrentProfilePicture = patient.User.ProfilePicture
            };
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(
            ClaimsPrincipal userPrincipal,
            PatientEditProfileViewModel model)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return (false, "Patient profile was not found.");
            }

            patient.User.FullName = model.FullName;

            // Email is intentionally not updated here.
            // Changing Identity email/username directly can break normalized Identity fields.
            // The patient can view the email, but account email changes should be handled separately.

            patient.BloodType = model.BloodType;
            patient.Address = model.Address;
            patient.EmergencyContactName = model.EmergencyContactName;
            patient.EmergencyContactPhone = model.EmergencyContactPhone;

            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.ProfileImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return (false, "Only JPG, PNG, or WEBP images are allowed.");
                }

                var imagesFolder = Path.Combine(_environment.WebRootPath, "images", "patients");

                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = $"patient-{patient.Id}-{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(imagesFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImageFile.CopyToAsync(stream);
                }

                patient.User.ProfilePicture = $"/images/patients/{fileName}";
            }

            await _context.SaveChangesAsync();

            return (true, "Profile updated successfully.");
        }

        public async Task<List<PatientAppointmentsViewModel>> GetAppointmentsAsync(ClaimsPrincipal userPrincipal)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return new List<PatientAppointmentsViewModel>();
            }

            return await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Select(a => new PatientAppointmentsViewModel
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString("HH:mm"),
                    EndTime = a.EndTime.ToString("HH:mm"),
                    DoctorName = a.Doctor.User.FullName,
                    Status = a.Status.Name,
                    Notes = a.Notes
                })
                .ToListAsync();
        }

        public async Task<List<PatientHistoryViewModel>> GetHistoryAsync(ClaimsPrincipal userPrincipal)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return new List<PatientHistoryViewModel>();
            }

            return await _context.VisitRecords
                .Include(v => v.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .Include(v => v.Prescriptions)
                .Where(v => v.Appointment.PatientId == patient.Id)
                .OrderByDescending(v => v.Appointment.AppointmentDate)
                .Select(v => new PatientHistoryViewModel
                {
                    AppointmentId = v.AppointmentId,
                    AppointmentDate = v.Appointment.AppointmentDate,
                    DoctorName = v.Appointment.Doctor.User.FullName,
                    Diagnosis = v.Diagnosis,
                    Treatment = v.Treatment,
                    DoctorNotes = v.DoctorNotes,
                    Prescriptions = v.Prescriptions.Select(p => new PrescriptionItemViewModel
                    {
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        Frequency = p.Frequency,
                        DurationDays = p.DurationDays,
                        Instructions = p.Instructions
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<PatientNotificationViewModel>> GetNotificationsAsync(ClaimsPrincipal userPrincipal)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return new List<PatientNotificationViewModel>();
            }

            return await _context.Notifications
                .Where(n => n.UserId == patient.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new PatientNotificationViewModel
                {
                    NotificationId = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                })
                .ToListAsync();
        }

        public async Task MarkAllNotificationsAsReadAsync(ClaimsPrincipal userPrincipal)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return;
            }

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == patient.UserId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PatientBookAppointmentViewModel?> GetBookAppointmentModelAsync(
            ClaimsPrincipal userPrincipal,
            int? specializationId = null,
            int? doctorId = null,
            DateTime? appointmentDate = null)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return null;
            }

            var model = new PatientBookAppointmentViewModel
            {
                SpecializationId = specializationId,
                DoctorId = doctorId,
                AppointmentDate = appointmentDate
            };

            await PopulateBookingOptionsAsync(model);

            return model;
        }

        public async Task<(bool Success, string Message)> BookAppointmentAsync(
            ClaimsPrincipal userPrincipal,
            PatientBookAppointmentViewModel model)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return (false, "Patient profile was not found.");
            }

            if (model.SpecializationId == null ||
                model.DoctorId == null ||
                model.AppointmentDate == null ||
                string.IsNullOrWhiteSpace(model.StartTime))
            {
                return (false, "Please complete all required appointment details.");
            }

            if (!TimeOnly.TryParse(model.StartTime, out var startTime))
            {
                return (false, "Invalid appointment time.");
            }

            var appointmentDate = model.AppointmentDate.Value.Date;

            if (appointmentDate < DateTime.Today)
            {
                return (false, "You cannot book an appointment in the past.");
            }

            if (appointmentDate == DateTime.Today &&
                startTime <= TimeOnly.FromDateTime(DateTime.Now))
            {
                return (false, "You cannot book a time slot that has already passed.");
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId.Value);

            if (doctor == null)
            {
                return (false, "Selected doctor was not found.");
            }

            if (!doctor.User.IsActive)
            {
                return (false, "Selected doctor account is not active.");
            }

            var doctorHasSpecialization = doctor.DoctorSpecializations
                .Any(ds => ds.SpecializationId == model.SpecializationId.Value);

            if (!doctorHasSpecialization)
            {
                return (false, "The selected doctor does not match the selected specialization.");
            }

            var matchingSchedules = doctor.Schedules
                .Where(s => s.DayOfWeek == appointmentDate.DayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToList();

            if (!matchingSchedules.Any())
            {
                return (false, "The selected doctor is not available on this day.");
            }

            var schedule = matchingSchedules.FirstOrDefault(s =>
                s.StartTime <= startTime &&
                startTime.AddMinutes(s.SlotDurationMinutes) <= s.EndTime);

            if (schedule == null)
            {
                return (false, "The selected time is outside the doctor's working hours.");
            }

            var endTime = startTime.AddMinutes(schedule.SlotDurationMinutes);

            var doctorOnLeave = doctor.Leaves.Any(l =>
                appointmentDate >= l.StartDate.Date &&
                appointmentDate <= l.EndDate.Date);

            if (doctorOnLeave)
            {
                return (false, "The selected doctor is on leave on this date.");
            }

            var existingAppointments = await _context.Appointments
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.AppointmentDate.Date == appointmentDate &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Missed")
                .ToListAsync();

            var hasConflict = existingAppointments.Any(a =>
                a.StartTime < endTime &&
                a.EndTime > startTime);

            if (hasConflict)
            {
                return (false, "This time slot is already booked. Please select another available slot.");
            }

            var requestedStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Requested");

            if (requestedStatus == null)
            {
                return (false, "Appointment status 'Requested' was not found in the database.");
            }

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                StatusId = requestedStatus.Id,
                AppointmentDate = appointmentDate,
                StartTime = startTime,
                EndTime = endTime,
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return (false, "This appointment could not be booked because the selected slot may no longer be available.");
            }

            await _notificationService.CreatePatientNotificationAsync(
                patient.Id,
                "Appointment Request Submitted",
                $"Your appointment request with Dr. {doctor.User.FullName} on {appointmentDate:dd MMM yyyy} at {startTime:HH:mm} has been submitted.",
                "Appointment",
                appointment.Id,
                "Appointment");

            await _notificationService.CreateDoctorNotificationAsync(
                doctor.Id,
                "New Appointment Request",
                $"{patient.User.FullName} requested an appointment on {appointmentDate:dd MMM yyyy} at {startTime:HH:mm}.",
                "Appointment",
                appointment.Id,
                "Appointment");

            return (true, "Appointment request submitted successfully.");
        }

        public async Task<(bool Success, string Message)> CancelAppointmentAsync(
            ClaimsPrincipal userPrincipal,
            int appointmentId)
        {
            var patient = await GetCurrentPatientAsync(userPrincipal);

            if (patient == null)
            {
                return (false, "Patient profile was not found.");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Status)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id);

            if (appointment == null)
            {
                return (false, "Appointment was not found.");
            }

            if (appointment.Status.Name != "Requested" &&
                appointment.Status.Name != "Confirmed")
            {
                return (false, "This appointment cannot be cancelled.");
            }

            var cancelledStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Cancelled");

            if (cancelledStatus == null)
            {
                return (false, "Appointment status 'Cancelled' was not found in the database.");
            }

            appointment.StatusId = cancelledStatus.Id;
            appointment.CancellationReason = "Cancelled by patient";
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.CreatePatientNotificationAsync(
                patient.Id,
                "Appointment Cancelled",
                $"Your appointment with Dr. {appointment.Doctor.User.FullName} on {appointment.AppointmentDate:dd MMM yyyy} at {appointment.StartTime:HH:mm} has been cancelled.",
                "Appointment",
                appointment.Id,
                "Appointment");

            await _notificationService.CreateDoctorNotificationAsync(
                appointment.DoctorId,
                "Appointment Cancelled",
                $"{patient.User.FullName} cancelled the appointment on {appointment.AppointmentDate:dd MMM yyyy} at {appointment.StartTime:HH:mm}.",
                "Appointment",
                appointment.Id,
                "Appointment");

            return (true, "Appointment cancelled successfully.");
        }

        private async Task<Patient?> GetCurrentPatientAsync(ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);

            if (user == null)
            {
                return null;
            }

            return await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
        }

        private async Task PopulateBookingOptionsAsync(PatientBookAppointmentViewModel model)
        {
            model.SpecializationOptions = await _context.Specializations
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = model.SpecializationId == s.Id
                })
                .ToListAsync();

            var doctorsQuery = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                .Where(d => d.User.IsActive)
                .AsQueryable();

            if (model.SpecializationId != null)
            {
                doctorsQuery = doctorsQuery.Where(d =>
                    d.DoctorSpecializations.Any(ds => ds.SpecializationId == model.SpecializationId.Value));
            }

            model.DoctorOptions = await doctorsQuery
                .OrderBy(d => d.User.FullName)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.User.FullName,
                    Selected = model.DoctorId == d.Id
                })
                .ToListAsync();

            model.AvailableSlotOptions = new List<SelectListItem>();

            if (model.SpecializationId == null ||
                model.DoctorId == null ||
                model.AppointmentDate == null)
            {
                return;
            }

            var appointmentDate = model.AppointmentDate.Value.Date;

            if (appointmentDate < DateTime.Today)
            {
                return;
            }

            var doctor = await _context.Doctors
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId.Value);

            if (doctor == null)
            {
                return;
            }

            var doctorHasSpecialization = doctor.DoctorSpecializations
                .Any(ds => ds.SpecializationId == model.SpecializationId.Value);

            if (!doctorHasSpecialization)
            {
                return;
            }

            var doctorOnLeave = doctor.Leaves.Any(l =>
                appointmentDate >= l.StartDate.Date &&
                appointmentDate <= l.EndDate.Date);

            if (doctorOnLeave)
            {
                return;
            }

            var schedules = doctor.Schedules
                .Where(s => s.DayOfWeek == appointmentDate.DayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToList();

            if (!schedules.Any())
            {
                return;
            }

            var bookedAppointments = await _context.Appointments
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == model.DoctorId.Value &&
                    a.AppointmentDate.Date == appointmentDate &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Missed")
                .Select(a => new
                {
                    a.StartTime,
                    a.EndTime
                })
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                var currentTime = schedule.StartTime;

                while (currentTime.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
                {
                    var slotStart = currentTime;
                    var slotEnd = currentTime.AddMinutes(schedule.SlotDurationMinutes);

                    var isPastSlot =
                        appointmentDate == DateTime.Today &&
                        slotStart <= TimeOnly.FromDateTime(DateTime.Now);

                    var slotHasConflict = bookedAppointments.Any(a =>
                        a.StartTime < slotEnd &&
                        a.EndTime > slotStart);

                    if (!isPastSlot && !slotHasConflict)
                    {
                        model.AvailableSlotOptions.Add(new SelectListItem
                        {
                            Value = slotStart.ToString("HH:mm"),
                            Text = $"{slotStart:HH\\:mm} - {slotEnd:HH\\:mm}",
                            Selected = model.StartTime == slotStart.ToString("HH:mm")
                        });
                    }

                    currentTime = currentTime.AddMinutes(schedule.SlotDurationMinutes);
                }
            }
        }
    }
}