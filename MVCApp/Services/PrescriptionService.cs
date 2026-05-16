using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Doctor;
using WebAPI.Data;

namespace MVCApp.Services
{
    // Business logic for prescription pages shown to doctors.
    public class PrescriptionService : IPrescriptionService
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Gets prescriptions written by the logged-in doctor.
        public async Task<List<DoctorPrescriptionViewModel>?> GetDoctorPrescriptionsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            var doctorExists = await _context.Doctors
                .AnyAsync(d => d.UserId == userId && d.User.IsActive);

            if (!doctorExists)
            {
                return null;
            }

            return await _context.Prescriptions
                .AsNoTracking()
                .Include(p => p.VisitRecord)
                    .ThenInclude(v => v.Appointment)
                        .ThenInclude(a => a.Patient)
                            .ThenInclude(p => p.User)
                .Where(p => p.VisitRecord.Appointment.Doctor.UserId == userId)
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
        }
    }
}
