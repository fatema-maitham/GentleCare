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

        // GET api/appointment/lookup
        // PUBLIC - no auth required
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
                .Include(a => a.Status)
                .Where(a => a.PatientId == patient.Id &&
                    a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentLookupResponseDTO
                {
                    AppointmentId = a.Id,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString(),
                    Status = a.Status.Name
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET api/appointment
        // Receptionist & ClinicManager only
        [HttpGet]
        [Authorize(Roles = "Receptionist,ClinicManager")]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString(),
                    EndTime = a.EndTime.ToString(),
                    Status = a.Status.Name,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET api/appointment/my
        // Patient sees their own appointments
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
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Status)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString(),
                    EndTime = a.EndTime.ToString(),
                    Status = a.Status.Name,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // PUT api/appointment/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Doctor,Receptionist,ClinicManager")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] UpdateAppointmentStatusDTO dto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            // Find new status from lookup table
            var newStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == dto.Status);

            if (newStatus == null)
                return BadRequest(new { message = "Invalid status value." });

            // Validate status transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Requested",  new List<string> { "Confirmed", "Cancelled" } },
                { "Confirmed",  new List<string> { "CheckedIn", "Cancelled" } },
                { "CheckedIn",  new List<string> { "InProgress" } },
                { "InProgress", new List<string> { "Completed", "Missed" } },
            };

            var currentStatusName = appointment.Status.Name;

            if (validTransitions.ContainsKey(currentStatusName) &&
                !validTransitions[currentStatusName].Contains(newStatus.Name))
                return BadRequest(new
                {
                    message =
                    $"Cannot transition from {currentStatusName} to {newStatus.Name}."
                });

            appointment.StatusId = newStatus.Id;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.CancellationReason))
                appointment.CancellationReason = dto.CancellationReason;

            await _context.SaveChangesAsync();

            // Create notification
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == appointment.PatientId);

            if (patient != null)
            {
                var notificationType = await _context.NotificationTypes
                    .FirstOrDefaultAsync(t => t.Name == "Appointment");

                var notification = new Notification
                {
                    UserId = patient.UserId,
                    Title = "Appointment Status Updated",
                    Message = $"Your appointment status is now {newStatus.Name}.",
                    NotificationTypeId = notificationType?.Id,
                    RelatedEntityId = appointment.Id,
                    RelatedEntityType = "Appointment",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            // SignalR broadcast
            var updatedAppointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (updatedAppointment != null)
            {
                await _hubService.NotifyAppointmentStatusChanged(
                    id,
                    updatedAppointment.Patient.User.FullName,
                    updatedAppointment.Doctor.User.FullName,
                    newStatus.Name);

                await _hubService.NotifyPatient(
                    updatedAppointment.PatientId,
                    "Appointment Update",
                    $"Your appointment is now {newStatus.Name}.");
            }

            return Ok(new
            {
                message =
                $"Appointment status updated to {newStatus.Name}."
            });
        }
    }
}