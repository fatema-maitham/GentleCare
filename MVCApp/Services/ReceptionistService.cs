using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Receptionist;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Services
{
    public class ReceptionistService : IReceptionistService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClinicNotificationService _notificationService;

        public ReceptionistService(
            ApplicationDbContext context,
            IClinicNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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
                    Text = s.Name
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
                .OrderByDescending(a => a.AppointmentDate)
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

            var doctorHasSpecialization = await _context.DoctorSpecializations
                .AnyAsync(ds => ds.DoctorId == doctorId && ds.SpecializationId == specializationId);

            if (!doctorHasSpecialization)
            {
                return (false, "Selected doctor does not match the selected specialization.");
            }

            var schedule = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId
                         && s.DayOfWeek == appointmentDate.DayOfWeek
                         && s.StartTime <= selectedStartTime)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return (false, "Selected time does not match doctor schedule.");
            }

            var endTime = selectedStartTime.AddMinutes(schedule.SlotDurationMinutes);

            if (endTime > schedule.EndTime)
            {
                return (false, "Selected time exceeds doctor schedule.");
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
                    selectedStartTime < a.EndTime &&
                    a.StartTime < endTime);

            if (hasConflict)
            {
                return (false, "This slot is already booked.");
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

            await _notificationService.CreatePatientNotificationAsync(
                patientId,
                "Appointment Booked",
                $"Your appointment has been booked with Dr. {doctorName} on {appointmentDate:dd MMM yyyy} at {selectedStartTime:HH:mm}.",
                "Appointment",
                appointment.Id,
                "Appointment");

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
            var allowedStatusNames = GetAllowedNextStatuses(oldStatus);
            var refreshedModel = BuildUpdateStatusModel(appointment);

            if (!allowedStatusNames.Contains(model.NewStatus))
            {
                return (false, "Invalid status transition.", refreshedModel);
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
                appointment.CancellationReason = model.CancellationReason;
            }
            else
            {
                appointment.CancellationReason = null;
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreatePatientNotificationAsync(
                appointment.PatientId,
                "Appointment Status Updated",
                $"Your appointment with Dr. {appointment.Doctor.User.FullName} changed from {oldStatus} to {model.NewStatus}.",
                "Appointment",
                appointment.Id,
                "Appointment");

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
            var allowedStatuses = GetAllowedNextStatuses(oldStatus);

            if (!allowedStatuses.Contains(newStatus))
            {
                return (false, "Invalid status update.");
            }

            var status = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == newStatus);

            if (status == null)
            {
                return (false, "Selected status was not found.");
            }

            appointment.StatusId = status.Id;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (newStatus != "Cancelled")
            {
                appointment.CancellationReason = null;
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreatePatientNotificationAsync(
                appointment.PatientId,
                "Appointment Status Updated",
                $"Your appointment with Dr. {appointment.Doctor.User.FullName} changed from {oldStatus} to {newStatus}.",
                "Appointment",
                appointment.Id,
                "Appointment");

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
                         && a.Status.Name != "Cancelled")
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                var current = schedule.StartTime;

                while (current.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
                {
                    var slotStart = current;
                    var slotEnd = current.AddMinutes(schedule.SlotDurationMinutes);

                    var overlaps = existingAppointments.Any(a =>
                        slotStart < a.EndTime && a.StartTime < slotEnd);

                    if (!overlaps)
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
            var allowedStatusNames = GetAllowedNextStatuses(appointment.Status.Name);

            return new ReceptionistUpdateAppointmentStatusViewModel
            {
                AppointmentId = appointment.Id,
                PatientName = appointment.Patient.User.FullName,
                DoctorName = appointment.Doctor.User.FullName,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime.ToString("HH:mm"),
                EndTime = appointment.EndTime.ToString("HH:mm"),
                CurrentStatus = appointment.Status.Name,
                AllowedStatuses = allowedStatusNames
                    .Select(s => new SelectListItem
                    {
                        Value = s,
                        Text = s
                    })
                    .ToList()
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

        private static List<string> GetAllowedNextStatuses(string currentStatus)
        {
            return currentStatus switch
            {
                "Requested" => new List<string> { "Confirmed", "Cancelled" },
                "Confirmed" => new List<string> { "CheckedIn", "Cancelled", "Missed" },
                "CheckedIn" => new List<string> { "InProgress", "Missed" },
                "InProgress" => new List<string> { "Completed" },
                _ => new List<string>()
            };
        }
    }
}