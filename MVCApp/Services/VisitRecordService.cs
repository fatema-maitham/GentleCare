using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Doctor;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Services
{
    // Business logic for creating and editing visit records.
    // It also handles prescription rows saved with a visit record.
    public class VisitRecordService : IVisitRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public VisitRecordService(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Opens the create form for an appointment owned by the logged-in doctor.
        public async Task<CreateVisitRecordViewModel?> GetCreateVisitRecordAsync(string userId, int appointmentId)
        {
            var appointment = await GetDoctorAppointmentAsync(userId, appointmentId, asTracking: false);
            if (appointment == null || appointment.VisitRecord != null || !CanCreateVisitRecord(appointment.Status.Name))
            {
                return null;
            }

            return new CreateVisitRecordViewModel
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
        }

        // Creates a visit record and optional prescriptions in one transaction.
        public async Task<(bool Success, string? ErrorMessage, int? AppointmentId)> CreateVisitRecordAsync(
            string userId,
            CreateVisitRecordViewModel model)
        {
            var appointment = await GetDoctorAppointmentAsync(userId, model.AppointmentId, asTracking: true);
            if (appointment == null)
            {
                return (false, "Appointment was not found.", null);
            }

            if (appointment.VisitRecord != null)
            {
                return (false, "A visit record already exists for this appointment.", appointment.Id);
            }

            if (!CanCreateVisitRecord(appointment.Status.Name))
            {
                return (false, "Visit record can only be created when the appointment is in progress or completed.", appointment.Id);
            }

            var cleanedPrescriptions = NormalizePrescriptionInputs(model.Prescriptions);
            var prescriptionError = ValidatePrescriptionInputs(cleanedPrescriptions);
            if (prescriptionError != null)
            {
                return (false, prescriptionError, appointment.Id);
            }

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
                    await _context.Prescriptions.AddRangeAsync(cleanedPrescriptions.Select(p => new Prescription
                    {
                        VisitRecordId = visitRecord.Id,
                        MedicationName = p.MedicationName.Trim(),
                        Dosage = p.Dosage.Trim(),
                        Frequency = p.Frequency.Trim(),
                        DurationDays = p.DurationDays,
                        Instructions = string.IsNullOrWhiteSpace(p.Instructions) ? null : p.Instructions.Trim(),
                        CreatedAt = DateTime.UtcNow
                    }));
                }

                if (appointment.Status.Name != "Completed")
                {
                    var completedStatus = await _context.AppointmentStatuses
                        .FirstAsync(s => s.Name == "Completed");

                    appointment.StatusId = completedStatus.Id;
                    appointment.UpdatedAt = DateTime.UtcNow;
                }

                if (cleanedPrescriptions.Any())
                {
                    await CreatePrescriptionNotificationAsync(appointment);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, null, appointment.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (false, "An error occurred while creating the visit record.", appointment.Id);
            }
        }

        // Opens the edit form for an existing visit record.
        public async Task<EditVisitRecordViewModel?> GetEditVisitRecordAsync(string userId, int appointmentId)
        {
            var appointment = await GetDoctorAppointmentAsync(userId, appointmentId, asTracking: false);
            if (appointment == null || appointment.VisitRecord == null)
            {
                return null;
            }

            return new EditVisitRecordViewModel
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
        }

        // Updates a visit record and replaces the prescription list with the latest entered rows.
        public async Task<(bool Success, string? ErrorMessage, int? AppointmentId)> EditVisitRecordAsync(
            string userId,
            EditVisitRecordViewModel model)
        {
            var appointment = await GetDoctorAppointmentAsync(userId, model.AppointmentId, asTracking: true);
            if (appointment == null)
            {
                return (false, "Appointment was not found.", null);
            }

            if (appointment.VisitRecord == null)
            {
                return (false, "No visit record exists for this appointment.", appointment.Id);
            }

            var cleanedPrescriptions = NormalizePrescriptionInputs(model.Prescriptions);
            var prescriptionError = ValidatePrescriptionInputs(cleanedPrescriptions);
            if (prescriptionError != null)
            {
                return (false, prescriptionError, appointment.Id);
            }

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
                    await _context.Prescriptions.AddRangeAsync(cleanedPrescriptions.Select(p => new Prescription
                    {
                        VisitRecordId = appointment.VisitRecord.Id,
                        MedicationName = p.MedicationName.Trim(),
                        Dosage = p.Dosage.Trim(),
                        Frequency = p.Frequency.Trim(),
                        DurationDays = p.DurationDays,
                        Instructions = string.IsNullOrWhiteSpace(p.Instructions) ? null : p.Instructions.Trim(),
                        CreatedAt = DateTime.UtcNow
                    }));
                }

                if (cleanedPrescriptions.Any())
                {
                    await CreatePrescriptionNotificationAsync(appointment);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, null, appointment.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (false, "An error occurred while updating the visit record.", appointment.Id);
            }
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

        private static bool CanCreateVisitRecord(string currentStatusName)
        {
            return currentStatusName == "InProgress" || currentStatusName == "Completed";
        }

        // Removes completely empty prescription rows before saving.
        private static List<PrescriptionInputViewModel> NormalizePrescriptionInputs(IEnumerable<PrescriptionInputViewModel>? prescriptions)
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
                    Instructions = string.IsNullOrWhiteSpace(p.Instructions) ? null : p.Instructions.Trim()
                })
                .ToList();
        }

        // If a prescription row is started, the important fields must be completed.
        private static string? ValidatePrescriptionInputs(List<PrescriptionInputViewModel> prescriptions)
        {
            for (var i = 0; i < prescriptions.Count; i++)
            {
                var item = prescriptions[i];

                if (string.IsNullOrWhiteSpace(item.MedicationName) ||
                    string.IsNullOrWhiteSpace(item.Dosage) ||
                    string.IsNullOrWhiteSpace(item.Frequency) ||
                    item.DurationDays <= 0)
                {
                    return $"Prescription row {i + 1} is incomplete. Medication, dosage, frequency, and duration are required.";
                }
            }

            return null;
        }

        private async Task CreatePrescriptionNotificationAsync(Appointment appointment)
        {
            await _notificationService.CreateNotificationAsync(
                appointment.Patient.UserId,
                "Prescription Updated",
                $"A prescription has been recorded for your visit on {appointment.AppointmentDate:dd MMM yyyy}.",
                "Prescription",
                appointment.Id,
                nameof(Prescription));
        }
    }
}
