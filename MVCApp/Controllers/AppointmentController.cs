using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCApp.ViewModels.Appointment;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MyAppointments()
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                .Where(a => a.DoctorId == doctor.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new DoctorAppointmentListItemViewModel
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString("HH\\:mm"),
                    EndTime = a.EndTime.ToString("HH\\:mm"),
                    Status = a.Status.Name,
                    Notes = a.Notes,
                    HasVisitRecord = a.VisitRecord != null
                })
                .ToListAsync();

            return View(appointments);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v.Prescriptions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var currentDoctor = await GetCurrentDoctorAsync();
            if (currentDoctor == null || appointment.DoctorId != currentDoctor.Id)
            {
                TempData["Error"] = "You can only access your own appointments.";
                return RedirectToAction(nameof(MyAppointments));
            }

            var vm = new DoctorAppointmentDetailsViewModel
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient.User.FullName,
                DoctorName = appointment.Doctor.User.FullName,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime.ToString("HH\\:mm"),
                EndTime = appointment.EndTime.ToString("HH\\:mm"),
                Status = appointment.Status.Name,
                Notes = appointment.Notes,
                CancellationReason = appointment.CancellationReason,
                VisitRecordId = appointment.VisitRecord?.Id,
                DoctorNotes = appointment.VisitRecord?.DoctorNotes,
                Diagnosis = appointment.VisitRecord?.Diagnosis,
                Treatment = appointment.VisitRecord?.Treatment,
                Prescriptions = appointment.VisitRecord?.Prescriptions
                    .Select(p => new PrescriptionItemViewModel
                    {
                        Id = p.Id,
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        Frequency = p.Frequency,
                        DurationDays = p.DurationDays,
                        Instructions = p.Instructions
                    })
                    .ToList() ?? new List<PrescriptionItemViewModel>(),
                AllowedNextStatuses = GetAllowedNextStatuses(appointment.Status.Name)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateStatus(UpdateAppointmentStatusViewModel vm)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == vm.AppointmentId);

            if (appointment == null) return NotFound();

            var currentDoctor = await GetCurrentDoctorAsync();
            if (currentDoctor == null || appointment.DoctorId != currentDoctor.Id)
            {
                TempData["Error"] = "You can only update your own appointments.";
                return RedirectToAction(nameof(MyAppointments));
            }

            var allowedStatuses = GetAllowedNextStatuses(appointment.Status.Name);
            if (!allowedStatuses.Contains(vm.Status))
            {
                TempData["Error"] = $"Cannot transition from {appointment.Status.Name} to {vm.Status}.";
                return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
            }

            if (vm.Status == "Cancelled" && string.IsNullOrWhiteSpace(vm.CancellationReason))
            {
                TempData["Error"] = "Cancellation reason is required when cancelling an appointment.";
                return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
            }

            var newStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == vm.Status);

            if (newStatus == null)
            {
                TempData["Error"] = "Selected status was not found.";
                return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
            }

            appointment.StatusId = newStatus.Id;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (vm.Status == "Cancelled")
                appointment.CancellationReason = vm.CancellationReason;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Appointment status updated to {vm.Status}.";
            return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateVisitRecord(int id)
        {
            var appointment = await LoadDoctorOwnedAppointmentAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found or not assigned to you.";
                return RedirectToAction(nameof(MyAppointments));
            }

            if (appointment.Status.Name != "Completed")
            {
                TempData["Error"] = "Visit record can only be created for completed appointments.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (appointment.VisitRecord != null)
            {
                TempData["Error"] = "This appointment already has a visit record.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new CreateVisitRecordViewModel
            {
                AppointmentId = appointment.Id,
                PatientName = appointment.Patient.User.FullName,
                AppointmentDate = appointment.AppointmentDate
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateVisitRecord(CreateVisitRecordViewModel vm)
        {
            var appointment = await LoadDoctorOwnedAppointmentAsync(vm.AppointmentId);
            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found or not assigned to you.";
                return RedirectToAction(nameof(MyAppointments));
            }

            if (appointment.Status.Name != "Completed")
            {
                TempData["Error"] = "Visit record can only be created for completed appointments.";
                return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
            }

            if (appointment.VisitRecord != null)
            {
                TempData["Error"] = "This appointment already has a visit record.";
                return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
            }

            if (!ModelState.IsValid)
            {
                vm.PatientName = appointment.Patient.User.FullName;
                vm.AppointmentDate = appointment.AppointmentDate;
                return View(vm);
            }

            _context.VisitRecords.Add(new VisitRecord
            {
                AppointmentId = vm.AppointmentId,
                DoctorNotes = vm.DoctorNotes,
                Diagnosis = vm.Diagnosis,
                Treatment = vm.Treatment,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Visit record created successfully.";
            return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddPrescription(int visitRecordId)
        {
            var visitRecord = await LoadDoctorOwnedVisitRecordAsync(visitRecordId);
            if (visitRecord == null)
            {
                TempData["Error"] = "Visit record not found or not assigned to you.";
                return RedirectToAction(nameof(MyAppointments));
            }

            var vm = new CreatePrescriptionViewModel
            {
                VisitRecordId = visitRecord.Id,
                AppointmentId = visitRecord.AppointmentId,
                PatientName = visitRecord.Appointment.Patient.User.FullName
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddPrescription(CreatePrescriptionViewModel vm)
        {
            var visitRecord = await LoadDoctorOwnedVisitRecordAsync(vm.VisitRecordId);
            if (visitRecord == null)
            {
                TempData["Error"] = "Visit record not found or not assigned to you.";
                return RedirectToAction(nameof(MyAppointments));
            }

            if (!ModelState.IsValid)
            {
                vm.AppointmentId = visitRecord.AppointmentId;
                vm.PatientName = visitRecord.Appointment.Patient.User.FullName;
                return View(vm);
            }

            _context.Prescriptions.Add(new Prescription
            {
                VisitRecordId = vm.VisitRecordId,
                MedicationName = vm.MedicationName,
                Dosage = vm.Dosage,
                Frequency = vm.Frequency,
                DurationDays = vm.DurationDays,
                Instructions = vm.Instructions,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Prescription added successfully.";
            return RedirectToAction(nameof(Details), new { id = visitRecord.AppointmentId });
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> PatientHistory(int patientId)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction(nameof(MyAppointments));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v.Prescriptions)
                .Where(a => a.PatientId == patientId && a.DoctorId == doctor.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            if (!appointments.Any())
            {
                TempData["Error"] = "No history found for this patient.";
                return RedirectToAction(nameof(MyAppointments));
            }

            return View(appointments);
        }

        private async Task<Doctor?> GetCurrentDoctorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);
        }

        private async Task<WebAPI.Models.Appointment?> LoadDoctorOwnedAppointmentAsync(int appointmentId)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return null;

            return await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);
        }

        private async Task<VisitRecord?> LoadDoctorOwnedVisitRecordAsync(int visitRecordId)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return null;

            return await _context.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Status)
                .Where(v => v.Appointment.DoctorId == doctor.Id)
                .FirstOrDefaultAsync(v => v.Id == visitRecordId);
        }

        private static List<string> GetAllowedNextStatuses(string currentStatus)
        {
            return currentStatus switch
            {
                "Requested" => new List<string> { "Confirmed", "Cancelled" },
                "Confirmed" => new List<string> { "CheckedIn", "Cancelled" },
                "CheckedIn" => new List<string> { "InProgress" },
                "InProgress" => new List<string> { "Completed", "Missed" },
                _ => new List<string>()
            };
        }
    }
}
