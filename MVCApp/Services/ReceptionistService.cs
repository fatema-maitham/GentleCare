using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Receptionist;
using System.Security.Claims;
using WebAPI.Data;
using WebAPI.Hubs;
using WebAPI.Models;

namespace MVCApp.Services
{
    public class ReceptionistService : IReceptionistService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClinicNotificationService _notificationService;
        private readonly IHubContext<AppointmentHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ReceptionistService(
            ApplicationDbContext context,
            IClinicNotificationService notificationService,
            IHubContext<AppointmentHub> hubContext,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<ReceptionistProfileViewModel?> GetProfileAsync(ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);

            if (user == null)
            {
                return null;
            }

            return new ReceptionistProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                ProfilePicture = user.ProfilePicture
            };
        }

        public async Task<ReceptionistEditProfileViewModel?> GetEditProfileAsync(ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);

            if (user == null)
            {
                return null;
            }

            return new ReceptionistEditProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                CurrentProfilePicture = user.ProfilePicture
            };
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(
            ClaimsPrincipal userPrincipal,
            ReceptionistEditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);

            if (user == null)
            {
                return (false, "Receptionist profile was not found.");
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.ProfileImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return (false, "Only JPG, PNG, or WEBP images are allowed.");
                }

                var imagesFolder = Path.Combine(_environment.WebRootPath, "images", "receptionists");

                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = $"receptionist-{user.Id}-{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(imagesFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImageFile.CopyToAsync(stream);
                }

                user.ProfilePicture = $"/images/receptionists/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return (false, "Profile could not be updated.");
            }

            return (true, "Profile updated successfully.");
        }

        public async Task<ReceptionistDashboardViewModel> GetDashboardAsync()
        {
            var today = DateTime.Today;

            var todayAppointmentsQuery = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Where(a => a.AppointmentDate.Date == today);

            var todayAppointments = await todayAppointmentsQuery
                .Select(a => new ReceptionistAppointmentListItemViewModel
                {
                    AppointmentId = a.Id,
                    PatientName = a.Patient.User.FullName,
                    CPRNumber = a.Patient.CPRNumber,
                    ReferenceNumber = a.Patient.ReferenceNumber,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString("HH:mm"),
                    EndTime = a.EndTime.ToString("HH:mm"),
                    Status = a.Status.Name,
                    Notes = a.Notes,
                    CancellationReason = a.CancellationReason
                })
                .ToListAsync();

            todayAppointments = todayAppointments
                .OrderBy(a => GetStatusOrder(a.Status))
                .ThenBy(a => a.StartTime)
                .ToList();

            return new ReceptionistDashboardViewModel
            {
                TodayTotalAppointments = await todayAppointmentsQuery.CountAsync(),
                TodayConfirmedCount = await todayAppointmentsQuery.CountAsync(a => a.Status.Name == "Confirmed"),
                TodayCheckedInCount = await todayAppointmentsQuery.CountAsync(a => a.Status.Name == "CheckedIn"),
                TodayInProgressCount = await todayAppointmentsQuery.CountAsync(a => a.Status.Name == "InProgress"),
                TodayCompletedCount = await todayAppointmentsQuery.CountAsync(a => a.Status.Name == "Completed"),
                TodayAppointments = todayAppointments
            };
        }

        public async Task<ReceptionistAppointmentsPageViewModel> GetAppointmentsAsync(
            string? searchText,
            DateTime? selectedDate,
            string? selectedStatus)
        {
            var model = new ReceptionistAppointmentsPageViewModel
            {
                SearchText = searchText,
                SelectedDate = selectedDate
            };

            model.StatusOptions = await _context.AppointmentStatuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = FormatStatusName(s.Name)
                })
                .ToListAsync();

            var query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim();

                query = query.Where(a =>
                    a.Patient.User.FullName.Contains(search) ||
                    a.Patient.CPRNumber.Contains(search) ||
                    a.Patient.ReferenceNumber.Contains(search) ||
                    a.Doctor.User.FullName.Contains(search));
            }

            if (selectedDate.HasValue)
            {
                var date = selectedDate.Value.Date;
                query = query.Where(a => a.AppointmentDate.Date == date);
            }

            if (!string.IsNullOrWhiteSpace(selectedStatus))
            {
                query = query.Where(a => a.Status.Name == selectedStatus);
            }

            model.Appointments = await query
                .OrderBy(a => a.Status.Name == "Requested" ? 1 :
                              a.Status.Name == "Confirmed" ? 2 :
                              a.Status.Name == "CheckedIn" ? 3 :
                              a.Status.Name == "InProgress" ? 4 :
                              a.Status.Name == "Completed" ? 5 :
                              a.Status.Name == "Cancelled" ? 6 :
                              a.Status.Name == "Missed" ? 7 : 8)
                .ThenBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new ReceptionistAppointmentListItemViewModel
                {
                    AppointmentId = a.Id,
                    PatientName = a.Patient.User.FullName,
                    CPRNumber = a.Patient.CPRNumber,
                    ReferenceNumber = a.Patient.ReferenceNumber,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString("HH:mm"),
                    EndTime = a.EndTime.ToString("HH:mm"),
                    Status = a.Status.Name,
                    Notes = a.Notes,
                    CancellationReason = a.CancellationReason
                })
                .ToListAsync();

            return model;
        }
        public async Task<ReceptionistBookAppointmentViewModel> GetBookAppointmentModelAsync(
            int? patientId,
            int? specializationId,
            int? doctorId,
            DateTime? appointmentDate)
        {
            var model = new ReceptionistBookAppointmentViewModel
            {
                PatientId = patientId,
                SpecializationId = specializationId,
                DoctorId = doctorId,
                AppointmentDate = appointmentDate ?? DateTime.Today.AddDays(1)
            };

            await PopulateBookAppointmentListsAsync(model);
            return model;
        }

        public async Task<(bool Success, string Message)> BookAppointmentAsync(
            ReceptionistBookAppointmentViewModel model)
        {
            if (!model.PatientId.HasValue) return (false, "Please select a patient.");
            if (!model.SpecializationId.HasValue) return (false, "Please select a specialization.");
            if (!model.DoctorId.HasValue) return (false, "Please select a doctor.");
            if (!model.AppointmentDate.HasValue) return (false, "Please select an appointment date.");

            if (model.AppointmentDate.Value.Date < DateTime.Today)
            {
                return (false, "Appointment date cannot be in the past.");
            }

            if (!TimeOnly.TryParse(model.SelectedStartTime, out var selectedStartTime))
            {
                return (false, "Please select a valid time slot.");
            }

            var doctorId = model.DoctorId.Value;
            var specializationId = model.SpecializationId.Value;
            var patientId = model.PatientId.Value;
            var appointmentDate = model.AppointmentDate.Value.Date;

            var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);

            if (!patientExists)
            {
                return (false, "Selected patient was not found.");
            }

            var doctorHasSpecialization = await _context.DoctorSpecializations
                .AnyAsync(ds => ds.DoctorId == doctorId && ds.SpecializationId == specializationId);

            if (!doctorHasSpecialization)
            {
                return (false, "Selected doctor does not match the selected specialization.");
            }

            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == appointmentDate.DayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var schedule = schedules.FirstOrDefault(s =>
                s.StartTime <= selectedStartTime &&
                selectedStartTime.AddMinutes(s.SlotDurationMinutes) <= s.EndTime);

            if (schedule == null)
            {
                return (false, "Selected time does not match the doctor's working schedule.");
            }

            var endTime = selectedStartTime.AddMinutes(schedule.SlotDurationMinutes);

            if (appointmentDate == DateTime.Today && selectedStartTime <= TimeOnly.FromDateTime(DateTime.Now))
            {
                return (false, "Selected time has already passed.");
            }

            var hasLeave = await _context.DoctorLeaves.AnyAsync(l =>
                l.DoctorId == doctorId &&
                appointmentDate >= l.StartDate.Date &&
                appointmentDate <= l.EndDate.Date);

            if (hasLeave)
            {
                return (false, "Doctor is on leave for the selected date.");
            }

            var hasConflict = await _context.Appointments
                .Include(a => a.Status)
                .AnyAsync(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date == appointmentDate &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Missed" &&
                    selectedStartTime < a.EndTime &&
                    a.StartTime < endTime);

            if (hasConflict)
            {
                return (false, "This slot is already booked. Please select another available slot.");
            }

            var confirmedStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Confirmed");

            if (confirmedStatus == null)
            {
                return (false, "Confirmed status was not found in the database.");
            }

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                StatusId = confirmedStatus.Id,
                AppointmentDate = appointmentDate,
                StartTime = selectedStartTime,
                EndTime = endTime,
                Notes = model.Notes,
                CancellationReason = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var doctorName = await GetDoctorNameAsync(doctorId);
            var patientName = await GetPatientNameAsync(patientId);

            await _notificationService.CreatePatientNotificationAsync(
                patientId,
                "Appointment Booked",
                $"Your appointment has been booked with Dr. {doctorName} on {appointmentDate:dd MMM yyyy} at {selectedStartTime:HH:mm}.",
                "Appointment",
                appointment.Id,
                "Appointment");

            await _notificationService.CreateDoctorNotificationAsync(
                doctorId,
                "New Appointment Booked",
                $"{patientName} has been booked for an appointment on {appointmentDate:dd MMM yyyy} at {selectedStartTime:HH:mm}.",
                "Appointment",
                appointment.Id,
                "Appointment");

            await BroadcastAppointmentUpdateAsync(appointment.Id);

            return (true, "Appointment booked successfully.");
        }

        public async Task<ReceptionistUpdateAppointmentStatusViewModel?> GetUpdateStatusModelAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return null;
            }

            return BuildUpdateStatusModel(appointment);
        }

        public async Task<(bool Success, string Message, ReceptionistUpdateAppointmentStatusViewModel? Model)> UpdateStatusAsync(
            ReceptionistUpdateAppointmentStatusViewModel model)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == model.AppointmentId);

            if (appointment == null)
            {
                return (false, "Appointment not found.", null);
            }

            var oldStatus = appointment.Status.Name;
            var allowedStatusNames = GetReceptionistAllowedNextStatuses(oldStatus);
            var refreshedModel = BuildUpdateStatusModel(appointment);

            if (!allowedStatusNames.Contains(model.NewStatus))
            {
                return (false, "Invalid status transition for receptionist workflow.", refreshedModel);
            }

            if (model.NewStatus == "Cancelled" && string.IsNullOrWhiteSpace(model.CancellationReason))
            {
                return (false, "Cancellation reason is required when cancelling an appointment.", refreshedModel);
            }

            var newStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == model.NewStatus);

            if (newStatus == null)
            {
                return (false, "Selected status was not found.", refreshedModel);
            }

            appointment.StatusId = newStatus.Id;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (model.NewStatus == "Cancelled")
            {
                appointment.CancellationReason = model.CancellationReason!.Trim();
            }
            else
            {
                appointment.CancellationReason = null;
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreatePatientNotificationAsync(
                appointment.PatientId,
                "Appointment Status Updated",
                $"Your appointment with Dr. {appointment.Doctor.User.FullName} changed from {FormatStatusName(oldStatus)} to {FormatStatusName(model.NewStatus)}.",
                "Appointment",
                appointment.Id,
                "Appointment");

            await _notificationService.CreateDoctorNotificationAsync(
                appointment.DoctorId,
                "Appointment Status Updated",
                $"{appointment.Patient.User.FullName}'s appointment changed from {FormatStatusName(oldStatus)} to {FormatStatusName(model.NewStatus)}.",
                "Appointment",
                appointment.Id,
                "Appointment");

            await BroadcastAppointmentUpdateAsync(appointment.Id);

            return (true, "Appointment status updated successfully.", null);
        }

        public async Task<ReceptionistPatientSearchViewModel> SearchPatientsAsync(string? searchText)
        {
            var model = new ReceptionistPatientSearchViewModel
            {
                SearchText = searchText
            };

            var query = _context.Patients
                .Include(p => p.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim();

                query = query.Where(p =>
                    p.User.FullName.Contains(search) ||
                    p.User.Email!.Contains(search) ||
                    p.CPRNumber.Contains(search) ||
                    p.ReferenceNumber.Contains(search));
            }

            model.Patients = await query
                .OrderBy(p => p.User.FullName)
                .Select(p => new ReceptionistPatientSearchResultViewModel
                {
                    PatientId = p.Id,
                    FullName = p.User.FullName,
                    Email = p.User.Email ?? "Not provided",
                    CPRNumber = p.CPRNumber,
                    ReferenceNumber = p.ReferenceNumber,
                    DateOfBirth = p.DateOfBirth.ToString("dd MMM yyyy"),
                    PhoneNumber = p.User.PhoneNumber ?? "Not provided"
                })
                .ToListAsync();

            return model;
        }

        public async Task<ReceptionistLiveQueueViewModel> GetLiveQueueAsync()
        {
            var today = DateTime.Today;

            var queueItems = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Where(a =>
                    a.AppointmentDate.Date == today &&
                    (
                        a.Status.Name == "Confirmed" ||
                        a.Status.Name == "CheckedIn" ||
                        a.Status.Name == "InProgress" ||
                        a.Status.Name == "Completed"
                    ))
                .Select(a => new ReceptionistLiveQueueItemViewModel
                {
                    AppointmentId = a.Id,
                    PatientName = a.Patient.User.FullName,
                    CPRNumber = a.Patient.CPRNumber,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString("HH:mm"),
                    EndTime = a.EndTime.ToString("HH:mm"),
                    Status = a.Status.Name,
                    Notes = a.Notes
                })
                .ToListAsync();

            queueItems = queueItems
                .OrderBy(q => GetStatusOrder(q.Status))
                .ThenBy(q => q.StartTime)
                .ToList();

            return new ReceptionistLiveQueueViewModel
            {
                ConfirmedCount = queueItems.Count(q => q.Status == "Confirmed"),
                CheckedInCount = queueItems.Count(q => q.Status == "CheckedIn"),
                InProgressCount = queueItems.Count(q => q.Status == "InProgress"),
                CompletedCount = queueItems.Count(q => q.Status == "Completed"),
                QueueItems = queueItems
            };
        }

        public async Task<(bool Success, string Message)> UpdateQueueStatusAsync(
            int appointmentId,
            string newStatus)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return (false, "Appointment was not found.");
            }

            var oldStatus = appointment.Status.Name;

            var allowedStatuses = oldStatus switch
            {
                "Confirmed" => new List<string> { "CheckedIn", "Missed" },
                "CheckedIn" => new List<string> { "InProgress" },
                _ => new List<string>()
            };

            if (!allowedStatuses.Contains(newStatus))
            {
                return (false, "Invalid queue update for receptionist workflow.");
            }

            var status = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == newStatus);

            if (status == null)
            {
                return (false, "Selected status was not found.");
            }

            appointment.StatusId = status.Id;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.CancellationReason = null;

            await _context.SaveChangesAsync();

            await _notificationService.CreatePatientNotificationAsync(
                appointment.PatientId,
                "Appointment Status Updated",
                $"Your appointment with Dr. {appointment.Doctor.User.FullName} changed from {FormatStatusName(oldStatus)} to {FormatStatusName(newStatus)}.",
                "Appointment",
                appointment.Id,
                "Appointment");

            await _notificationService.CreateDoctorNotificationAsync(
                appointment.DoctorId,
                "Appointment Status Updated",
                $"{appointment.Patient.User.FullName}'s appointment changed from {FormatStatusName(oldStatus)} to {FormatStatusName(newStatus)}.",
                "Appointment",
                appointment.Id,
                "Appointment");

            await BroadcastAppointmentUpdateAsync(appointment.Id);

            return (true, "Queue status updated successfully.");
        }

        private async Task PopulateBookAppointmentListsAsync(ReceptionistBookAppointmentViewModel model)
        {
            model.Patients = await _context.Patients
                .Include(p => p.User)
                .OrderBy(p => p.User.FullName)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.User.FullName + " - " + p.CPRNumber,
                    Selected = model.PatientId == p.Id
                })
                .ToListAsync();

            model.Specializations = await _context.Specializations
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = model.SpecializationId == s.Id
                })
                .ToListAsync();

            model.Doctors = new List<SelectListItem>();
            model.AvailableSlots = new List<SelectListItem>();

            if (model.SpecializationId.HasValue)
            {
                var doctorRows = await _context.DoctorSpecializations
                    .Where(ds => ds.SpecializationId == model.SpecializationId.Value)
                    .Include(ds => ds.Doctor).ThenInclude(d => d.User)
                    .Select(ds => new
                    {
                        ds.DoctorId,
                        DoctorName = ds.Doctor.User.FullName
                    })
                    .Distinct()
                    .OrderBy(x => x.DoctorName)
                    .ToListAsync();

                model.Doctors = doctorRows
                    .Select(d => new SelectListItem
                    {
                        Value = d.DoctorId.ToString(),
                        Text = d.DoctorName,
                        Selected = model.DoctorId == d.DoctorId
                    })
                    .ToList();
            }

            if (model.DoctorId.HasValue && model.AppointmentDate.HasValue)
            {
                model.AvailableSlots = await BuildAvailableSlotsAsync(
                    model.DoctorId.Value,
                    model.AppointmentDate.Value.Date);
            }
        }

        private async Task<List<SelectListItem>> BuildAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var slots = new List<SelectListItem>();

            if (date.Date < DateTime.Today)
            {
                return slots;
            }

            var hasLeave = await _context.DoctorLeaves.AnyAsync(l =>
                l.DoctorId == doctorId &&
                date.Date >= l.StartDate.Date &&
                date.Date <= l.EndDate.Date);

            if (hasLeave)
            {
                return slots;
            }

            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == date.DayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var existingAppointments = await _context.Appointments
                .Include(a => a.Status)
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == date.Date
                         && a.Status.Name != "Cancelled"
                         && a.Status.Name != "Missed")
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                var current = schedule.StartTime;

                while (current.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
                {
                    var slotStart = current;
                    var slotEnd = current.AddMinutes(schedule.SlotDurationMinutes);

                    var isPastSlot = date.Date == DateTime.Today &&
                                     slotStart <= TimeOnly.FromDateTime(DateTime.Now);

                    var overlaps = existingAppointments.Any(a =>
                        slotStart < a.EndTime && a.StartTime < slotEnd);

                    if (!isPastSlot && !overlaps)
                    {
                        slots.Add(new SelectListItem
                        {
                            Value = slotStart.ToString("HH:mm"),
                            Text = $"{slotStart:hh:mm tt} - {slotEnd:hh:mm tt}"
                        });
                    }

                    current = current.AddMinutes(schedule.SlotDurationMinutes);
                }
            }

            return slots;
        }

        private ReceptionistUpdateAppointmentStatusViewModel BuildUpdateStatusModel(Appointment appointment)
        {
            var allowedStatusNames = GetReceptionistAllowedNextStatuses(appointment.Status.Name);

            return new ReceptionistUpdateAppointmentStatusViewModel
            {
                AppointmentId = appointment.Id,
                PatientName = appointment.Patient.User.FullName,
                DoctorName = appointment.Doctor.User.FullName,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime.ToString("HH:mm"),
                EndTime = appointment.EndTime.ToString("HH:mm"),
                CurrentStatus = FormatStatusName(appointment.Status.Name),
                AllowedStatuses = allowedStatusNames
                    .Select(s => new SelectListItem
                    {
                        Value = s,
                        Text = FormatStatusName(s)
                    })
                    .ToList()
            };
        }

        private static List<string> GetReceptionistAllowedNextStatuses(string currentStatus)
        {
            return currentStatus switch
            {
                "Requested" => new List<string> { "Confirmed", "Cancelled" },
                "Confirmed" => new List<string> { "CheckedIn", "Cancelled", "Missed" },
                "CheckedIn" => new List<string> { "InProgress", "Cancelled" },

                // Receptionist should not complete medical visits.
                // Completed should happen after the doctor creates/finishes the visit record.
                "InProgress" => new List<string>(),

                _ => new List<string>()
            };
        }

        private async Task<string> GetDoctorNameAsync(int doctorId)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.Id == doctorId)
                .Select(d => d.User.FullName)
                .FirstOrDefaultAsync() ?? "your doctor";
        }

        private async Task<string> GetPatientNameAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.User)
                .Where(p => p.Id == patientId)
                .Select(p => p.User.FullName)
                .FirstOrDefaultAsync() ?? "The patient";
        }

        private async Task BroadcastAppointmentUpdateAsync(int appointmentId)
        {
            await _hubContext.Clients.Group("ReceptionistQueue")
                .SendAsync("QueueUpdated");

            await _hubContext.Clients.All
                .SendAsync("AppointmentUpdated", appointmentId);
        }

        private static int GetStatusOrder(string status)
        {
            return status switch
            {
                "Requested" => 1,
                "Confirmed" => 2,
                "CheckedIn" => 3,
                "InProgress" => 4,
                "Completed" => 5,
                "Cancelled" => 6,
                "Missed" => 7,
                _ => 8
            };
        }

        private static string FormatStatusName(string statusName)
        {
            return statusName switch
            {
                "CheckedIn" => "Checked In",
                "InProgress" => "In Progress",
                _ => statusName
            };
        }
    }
}