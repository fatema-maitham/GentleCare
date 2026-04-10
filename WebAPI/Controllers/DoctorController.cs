using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Schedules)
                .Select(d => new DoctorResponseDTO
                {
                    Id = d.Id,
                    FullName = d.User.FullName,
                    Email = d.User.Email!,
                    LicenseNumber = d.LicenseNumber,
                    Bio = d.Bio,
                    Specializations = d.DoctorSpecializations
                        .Select(ds => ds.Specialization.Name)
                        .ToList(),
                    Schedules = d.Schedules.Select(s => new ScheduleDTO
                    {
                        DayOfWeek = s.DayOfWeek.ToString(),
                        StartTime = s.StartTime.ToString(),
                        EndTime = s.EndTime.ToString(),
                        SlotDurationMinutes = s.SlotDurationMinutes
                    }).ToList()
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            var result = new DoctorResponseDTO
            {
                Id = doctor.Id,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email!,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                Specializations = doctor.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name)
                    .ToList(),
                Schedules = doctor.Schedules.Select(s => new ScheduleDTO
                {
                    DayOfWeek = s.DayOfWeek.ToString(),
                    StartTime = s.StartTime.ToString(),
                    EndTime = s.EndTime.ToString(),
                    SlotDurationMinutes = s.SlotDurationMinutes
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ClinicManager")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDTO dto)
        {
            var user = await _context.Users
                .FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var exists = await _context.Doctors
                .AnyAsync(d => d.UserId == dto.UserId);
            if (exists)
                return BadRequest(new
                {
                    message =
                    "Doctor profile already exists for this user."
                });

            var doctor = new Doctor
            {
                UserId = dto.UserId,
                LicenseNumber = dto.LicenseNumber,
                Bio = dto.Bio
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            foreach (var specId in dto.SpecializationIds)
            {
                _context.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorId = doctor.Id,
                    SpecializationId = specId
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Doctor profile created.",
                doctorId = doctor.Id
            });
        }

        [HttpGet("{id}/availability")]
        [Authorize]
        public async Task<IActionResult> GetAvailability(
            int id, [FromQuery] DateTime date)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

 
            var onLeave = doctor.Leaves.Any(l =>
                date >= l.StartDate && date <= l.EndDate);

            if (onLeave)
                return Ok(new
                {
                    available = false,
                    message = "Doctor is on leave.",
                    slots = new List<string>()
                });

        
            var schedule = doctor.Schedules
                .FirstOrDefault(s => s.DayOfWeek == date.DayOfWeek);

            if (schedule == null)
                return Ok(new
                {
                    available = false,
                    message = "Doctor does not work on this day.",
                    slots = new List<string>()
                });

    
            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == id &&
                    a.AppointmentDate.Date == date.Date &&
                    a.StatusId != 6 &&
                    a.StatusId != 7)
                .Select(a => a.StartTime)
                .ToListAsync();

            var slots = new List<string>();
            var current = schedule.StartTime;

            while (current < schedule.EndTime)
            {
                if (!bookedSlots.Contains(current))
                    slots.Add(current.ToString());

                current = current.AddMinutes(schedule.SlotDurationMinutes);
            }

            return Ok(new
            {
                available = slots.Any(),
                message = "Availability retrieved.",
                slots
            });
        }
    }
}