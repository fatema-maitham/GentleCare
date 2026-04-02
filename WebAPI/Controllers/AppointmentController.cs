using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationHubService _hubService;

        public AppointmentController(
            ApplicationDbContext context,
            NotificationHubService hubService)
        {
            _context = context;
            _hubService = hubService;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> PublicLookup(
            [FromQuery] string cprNumber,
            [FromQuery] string referenceNumber)
        {
            if (string.IsNullOrEmpty(cprNumber) ||
                string.IsNullOrEmpty(referenceNumber))
                return BadRequest(new
                {
                    message =
                    "CPR number and reference number are required."
                });

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p =>
                    p.CPRNumber == cprNumber &&
                    p.ReferenceNumber == referenceNumber);

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Specialization)
                .Where(a => a.PatientId == patient.Id &&
                    a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentLookupResponseDTO
                {
                    AppointmentId = a.Id,
                    DoctorName = a.Doctor.User.FullName,
                    Specialization = a.Specialization.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString(),
                    Status = a.Status.ToString()
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist,ClinicManager")]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Specialization)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    Specialization = a.Specialization.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString(),
                    EndTime = a.EndTime.ToString(),
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound(new { message = "Patient profile not found." });

            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Specialization)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    Specialization = a.Specialization.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString(),
                    EndTime = a.EndTime.ToString(),
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Doctor,Receptionist,ClinicManager")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] UpdateAppointmentStatusDTO dto)
        {
            var appointment = await _context.Appointments
                .FindAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            if (!Enum.TryParse<AppointmentStatus>(dto.Status, out var newStatus))
                return BadRequest(new { message = "Invalid status value." });

            var validTransitions = new Dictionary<AppointmentStatus,
                List<AppointmentStatus>>
            {
                { AppointmentStatus.Requested, new List<AppointmentStatus>
                    { AppointmentStatus.Confirmed,
                      AppointmentStatus.Cancelled } },
                { AppointmentStatus.Confirmed, new List<AppointmentStatus>
                    { AppointmentStatus.CheckedIn,
                      AppointmentStatus.Cancelled } },
                { AppointmentStatus.CheckedIn, new List<AppointmentStatus>
                    { AppointmentStatus.InProgress } },
                { AppointmentStatus.InProgress, new List<AppointmentStatus>
                    { AppointmentStatus.Completed,
                      AppointmentStatus.Missed } },
            };

            if (validTransitions.ContainsKey(appointment.Status) &&
                !validTransitions[appointment.Status].Contains(newStatus))
                return BadRequest(new
                {
                    message =
                    $"Cannot transition from {appointment.Status} to {newStatus}."
                });

            appointment.Status = newStatus;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.CancellationReason))
                appointment.CancellationReason = dto.CancellationReason;

            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                UserId = (await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p =>
                        p.Id == appointment.PatientId))!.UserId,
                Title = "Appointment Status Updated",
                Message = $"Your appointment status has been updated to {newStatus}.",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var updatedAppointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (updatedAppointment != null)
            {
               
                await _hubService.NotifyAppointmentStatusChanged(
                    id,
                    updatedAppointment.Patient.User.FullName,
                    updatedAppointment.Doctor.User.FullName,
                    newStatus.ToString()
                );

                await _hubService.NotifyPatient(
                    updatedAppointment.PatientId,
                    "Appointment Update",
                    $"Your appointment is now {newStatus}."
                );
            }

            return Ok(new
            {
                message =
                $"Appointment status updated to {newStatus}."
            });
        }
    }
}