using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCApp.ViewModels.Patient;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return NotFound("Patient profile not found.");
            }

            var model = new PatientProfileViewModel
            {
                FullName = patient.User.FullName,
                Email = patient.User.Email ?? string.Empty,
                CPRNumber = patient.CPRNumber,
                ReferenceNumber = patient.ReferenceNumber,
                DateOfBirth = patient.DateOfBirth,
                BloodType = patient.BloodType,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return NotFound("Patient profile not found.");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Select(a => new PatientAppointmentsViewModel
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString(),
                    EndTime = a.EndTime.ToString(),
                    DoctorName = a.Doctor.User.FullName,
                    Status = a.Status.Name,
                    Notes = a.Notes
                })
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return NotFound("Patient profile not found.");
            }

            var history = await _context.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
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

            return View(history);
        }
    }
}