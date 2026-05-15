using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Doctor;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Services
{
    // Business logic for doctor appointments.
    // This keeps filtering, details, status workflow, and patient history outside controllers.
    public class DoctorAppointmentService : IDoctorAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentWorkflowService _workflowService;
        private readonly INotificationService _notificationService;

        public DoctorAppointmentService(
            ApplicationDbContext context,
            IAppointmentWorkflowService workflowService,
            INotificationService notificationService)
        {
            _context = context;
            _workflowService = workflowService;
            _notificationService = notificationService;
        }

        // Gets appointment list for the logged-in doctor with optional filters.
        public async Task<DoctorAppointmentListViewModel?> GetAppointmentsAsync(
            string userId,
            string? searchTerm,
            string? status,
            DateTime? date)
        {
            var doctor = await GetCurrentDoctorAsync(userId);
            if (doctor == null)
            {
                return null;
            }

            searchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
            status = string.IsNullOrWhiteSpace(status) ? null : _workflowService.NormalizeStatusName(status.Trim());

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

            return new DoctorAppointmentListViewModel
            {
                DoctorFullName = doctor.User.FullName,
                SearchTerm = searchTerm,
                SelectedStatus = status,
                SelectedDate = date,
                StatusOptions = await BuildStatusOptionsAsync(status),
                Appointments = appointments.Select(a => new DoctorAppointmentListItemViewModel
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    PatientId = a.PatientId,
                    PatientFullName = a.Patient.User.FullName,
                    PatientReferenceNumber = a.Patient.ReferenceNumber,
                    StatusName = _workflowService.FormatStatusName(a.Status.Name),
                    Notes = a.Notes
                }).ToList()
            };
        }

        // Gets one appointment details page for the logged-in doctor.
        public async Task<DoctorAppointmentDetailsViewModel?> GetAppointmentDetailsAsync(string userId, int appointmentId)
        {
            var appointment = await GetDoctorAppointmentAsync(userId, appointmentId, asTracking: false);
            if (appointment == null)
            {
                return null;
            }

            var currentStatus = appointment.Status.Name;
            var allowedNextStatuses = BuildAllowedStatusSelectList(currentStatus);

            return new DoctorAppointmentDetailsViewModel
            {
                AppointmentId = appointment.Id,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                StatusName = _workflowService.FormatStatusName(currentStatus),
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
                CanCreateVisitRecord = appointment.VisitRecord == null && CanCreateVisitRecord(currentStatus),
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
        }

        // Builds the update-status form with only valid next statuses.
        public async Task<UpdateAppointmentStatusViewModel?> GetUpdateStatusModelAsync(string userId, int appointmentId)
        {
            var appointment = await GetDoctorAppointmentAsync(userId, appointmentId, asTracking: false);
            if (appointment == null)
            {
                return null;
            }

            return new UpdateAppointmentStatusViewModel
            {
                AppointmentId = appointment.Id,
                CurrentStatusName = _workflowService.FormatStatusName(appointment.Status.Name),
                NewStatusName = string.Empty,
                AvailableStatuses = BuildAllowedStatusSelectList(appointment.Status.Name)
            };
        }

        // Updates status after checking appointment ownership and valid workflow transition.
        public async Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(
            string userId,
            UpdateAppointmentStatusViewModel model)
        {
            var appointment = await GetDoctorAppointmentAsync(userId, model.AppointmentId, asTracking: true);
            if (appointment == null)
            {
                return (false, "Appointment was not found.");
            }

            var newStatus = _workflowService.NormalizeStatusName(model.NewStatusName);
            if (string.IsNullOrWhiteSpace(newStatus))
            {
                return (false, "Please select a new status.");
            }

            if (!GetDoctorAllowedNextStatuses(appointment.Status.Name).Contains(newStatus))
            {
                return (false, "Invalid status transition for doctor workflow.");
            }

            if (newStatus == "Missed" && !CanMarkAsMissed(appointment))
            {
                return (false, "The appointment can only be marked as missed after its scheduled time has passed.");
            }

            var newStatusEntity = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == newStatus);

            if (newStatusEntity == null)
            {
                return (false, "Selected status does not exist in the database.");
            }

            var oldStatusName = appointment.Status.Name;
            appointment.StatusId = newStatusEntity.Id;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _notificationService.CreateNotificationAsync(
                appointment.Patient.UserId,
                "Appointment Status Updated",
                $"Your appointment on {appointment.AppointmentDate:dd MMM yyyy} changed from {_workflowService.FormatStatusName(oldStatusName)} to {_workflowService.FormatStatusName(newStatusEntity.Name)}.",
                "Appointment",
                appointment.Id,
                nameof(Appointment));

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // Gets patient history only if the logged-in doctor has an appointment with that patient.
        public async Task<DoctorPatientHistoryViewModel?> GetPatientHistoryAsync(string userId, int patientId)
        {
            var doctor = await GetCurrentDoctorAsync(userId);
            if (doctor == null)
            {
                return null;
            }

            var patient = await _context.Patients
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return null;
            }

            var hasRelationship = await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.DoctorId == doctor.Id && a.PatientId == patientId);

            if (!hasRelationship)
            {
                return null;
            }

            var visits = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v!.Prescriptions)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.PatientId == patientId &&
                    a.VisitRecord != null)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .ToListAsync();

            return new DoctorPatientHistoryViewModel
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
                    StatusName = _workflowService.FormatStatusName(a.Status.Name),
                    DoctorNotes = a.VisitRecord!.DoctorNotes,
                    Diagnosis = a.VisitRecord.Diagnosis,
                    Treatment = a.VisitRecord.Treatment,
                    PrescriptionCount = a.VisitRecord.Prescriptions.Count
                }).ToList()
            };
        }

        private async Task<Doctor?> GetCurrentDoctorAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.User.IsActive);
        }

        private async Task<Appointment?> GetDoctorAppointmentAsync(string userId, int appointmentId, bool asTracking)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            var query = _context.Appointments.AsQueryable();
            if (!asTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v!.Prescriptions)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    a.Doctor.UserId == userId &&
                    a.Doctor.User.IsActive);
        }

        private async Task<List<SelectListItem>> BuildStatusOptionsAsync(string? selectedStatus)
        {
            return await _context.AppointmentStatuses
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = _workflowService.FormatStatusName(s.Name),
                    Selected = s.Name == selectedStatus
                })
                .ToListAsync();
        }

        private List<SelectListItem> BuildAllowedStatusSelectList(string currentStatusName)
        {
            return GetDoctorAllowedNextStatuses(currentStatusName)
                .Select(status => new SelectListItem
                {
                    Value = status,
                    Text = _workflowService.FormatStatusName(status)
                })
                .ToList();
        }


        // Doctors can only move appointments through consultation progress statuses.
        private static List<string> GetDoctorAllowedNextStatuses(string currentStatusName)
        {
            return currentStatusName switch
            {
                "Confirmed" => new List<string> { "CheckedIn", "Missed" },
                "CheckedIn" => new List<string> { "InProgress" },
                "InProgress" => new List<string> { "Completed" },
                _ => new List<string>()
            };
        }

        private static bool CanCreateVisitRecord(string currentStatusName)
        {
            return currentStatusName == "InProgress" || currentStatusName == "Completed";
        }

        private static bool CanMarkAsMissed(Appointment appointment)
        {
            var appointmentEndDateTime = appointment.AppointmentDate.Date
                .Add(appointment.EndTime.ToTimeSpan());

            return DateTime.Now >= appointmentEndDateTime;
        }
    }
}
