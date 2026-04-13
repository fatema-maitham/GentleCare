using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        [Authorize(Roles = "Patient,Receptionist,ClinicManager")]
        public async Task<IActionResult> Create([FromBody] CreatePatientDTO dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

           
            var exists = await _context.Patients
                .AnyAsync(p => p.UserId == dto.UserId);
            if (exists)
                return BadRequest(new
                {
                    message =
                    "Patient profile already exists for this user."
                });

            var cprExists = await _context.Patients
                .AnyAsync(p => p.CPRNumber == dto.CPRNumber);
            if (cprExists)
                return BadRequest(new
                {
                    message =
                    "CPR number already registered."
                });

            
            var referenceNumber = $"REF-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var patient = new Patient
            {
                UserId = dto.UserId,
                CPRNumber = dto.CPRNumber,
                ReferenceNumber = referenceNumber,
                DateOfBirth = dto.DateOfBirth,
                BloodType = dto.BloodType,
                Address = dto.Address,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Patient profile created successfully.",
                patientId = patient.Id,
                referenceNumber = patient.ReferenceNumber
            });
        }

       
        [HttpGet]
        [Authorize(Roles = "Receptionist,ClinicManager")]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Select(p => new PatientResponseDTO
                {
                    PatientId = p.Id,
                    FullName = p.User.FullName,
                    Email = p.User.Email!,
                    CPRNumber = p.CPRNumber,
                    ReferenceNumber = p.ReferenceNumber,
                    DateOfBirth = p.DateOfBirth,
                    BloodType = p.BloodType,
                    Address = p.Address,
                    EmergencyContactName = p.EmergencyContactName,
                    EmergencyContactPhone = p.EmergencyContactPhone,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(patients);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Receptionist,ClinicManager,Doctor")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            return Ok(new PatientResponseDTO
            {
                PatientId = patient.Id,
                FullName = patient.User.FullName,
                Email = patient.User.Email!,
                CPRNumber = patient.CPRNumber,
                ReferenceNumber = patient.ReferenceNumber,
                DateOfBirth = patient.DateOfBirth,
                BloodType = patient.BloodType,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                CreatedAt = patient.CreatedAt
            });
        }

 
        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound(new { message = "Patient profile not found." });

            return Ok(new PatientResponseDTO
            {
                PatientId = patient.Id,
                FullName = patient.User.FullName,
                Email = patient.User.Email!,
                CPRNumber = patient.CPRNumber,
                ReferenceNumber = patient.ReferenceNumber,
                DateOfBirth = patient.DateOfBirth,
                BloodType = patient.BloodType,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                CreatedAt = patient.CreatedAt
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Patient,Receptionist,ClinicManager")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdatePatientDTO dto)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Patient" && patient.UserId != userId)
                return Forbid();

            patient.BloodType = dto.BloodType;
            patient.Address = dto.Address;
            patient.EmergencyContactName = dto.EmergencyContactName;
            patient.EmergencyContactPhone = dto.EmergencyContactPhone;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient profile updated successfully." });
        }

 
        [HttpGet("{id}/history")]
        [Authorize(Roles = "Doctor,Receptionist,ClinicManager")]
        public async Task<IActionResult> GetVisitHistory(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            var history = await _context.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(v => v.Prescriptions)
                .Where(v => v.Appointment.PatientId == id)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new
                {
                    VisitRecordId = v.Id,
                    AppointmentDate = v.Appointment.AppointmentDate,
                    DoctorName = v.Appointment.Doctor.User.FullName,
                    DoctorNotes = v.DoctorNotes,
                    Diagnosis = v.Diagnosis,
                    Treatment = v.Treatment,
                    CreatedAt = v.CreatedAt,
                    Prescriptions = v.Prescriptions.Select(p => new
                    {
                        p.MedicationName,
                        p.Dosage,
                        p.Frequency,
                        p.DurationDays,
                        p.Instructions
                    })
                })
                .ToListAsync();

            return Ok(history);
        }
    }
}