using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ClinicManager")]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/report/appointment-stats
        [HttpGet("appointment-stats")]
        public async Task<IActionResult> GetAppointmentStats(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var query = _context.Appointments.AsQueryable();

            if (from.HasValue)
                query = query.Where(a => a.AppointmentDate >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.AppointmentDate <= to.Value);

            var total = await query.CountAsync();
            if (total == 0)
                return Ok(new AppointmentStatsDTO());

            var completed = await query.CountAsync(a => a.StatusId == 5);
            var cancelled = await query.CountAsync(a => a.StatusId == 6);
            var missed = await query.CountAsync(a => a.StatusId == 7);
            var pending = await query.CountAsync(a =>
                a.StatusId == 1 || a.StatusId == 2 ||
                a.StatusId == 3 || a.StatusId == 4);

            return Ok(new AppointmentStatsDTO
            {
                TotalAppointments = total,
                Completed = completed,
                Cancelled = cancelled,
                Missed = missed,
                Pending = pending,
                CancellationRate = Math.Round((double)cancelled / total * 100, 2),
                MissedRate = Math.Round((double)missed / total * 100, 2)
            });
        }

        // GET api/report/doctor-workload
        [HttpGet("doctor-workload")]
        public async Task<IActionResult> GetDoctorWorkload(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Appointments)
                .ToListAsync();

            var result = doctors.Select(d =>
            {
                var appts = d.Appointments
                    .Where(a =>
                        (!from.HasValue || a.AppointmentDate >= from.Value) &&
                        (!to.HasValue || a.AppointmentDate <= to.Value))
                    .ToList();

                return new DoctorWorkloadDTO
                {
                    DoctorName = d.User.FullName,
                    Specializations = d.DoctorSpecializations
                        .Select(ds => ds.Specialization.Name).ToList(),
                    TotalAppointments = appts.Count,
                    CompletedAppointments = appts.Count(a => a.StatusId == 5),
                    CancelledAppointments = appts.Count(a => a.StatusId == 6),
                    MissedAppointments = appts.Count(a => a.StatusId == 7)
                };
            }).ToList();

            return Ok(result);
        }

        // GET api/report/specialization-stats
        [HttpGet("specialization-stats")]
        public async Task<IActionResult> GetSpecializationStats(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var doctors = await _context.Doctors
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Appointments)
                .ToListAsync();

            var result = doctors
                .SelectMany(d => d.DoctorSpecializations.Select(ds => new
                {
                    SpecializationName = ds.Specialization.Name,
                    Appointments = d.Appointments
                        .Where(a =>
                            (!from.HasValue || a.AppointmentDate >= from.Value) &&
                            (!to.HasValue || a.AppointmentDate <= to.Value))
                        .ToList()
                }))
                .GroupBy(x => x.SpecializationName)
                .Select(g => new SpecializationStatsDTO
                {
                    SpecializationName = g.Key,
                    TotalAppointments = g.Sum(x => x.Appointments.Count),
                    CompletedAppointments = g.Sum(x =>
                        x.Appointments.Count(a => a.StatusId == 5))
                })
                .ToList();

            return Ok(result);
        }

        // GET api/report/daily-summary
        [HttpGet("daily-summary")]
        public async Task<IActionResult> GetDailySummary()
        {
            var today = DateTime.Today;

            var todayAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == today)
                .ToListAsync();

            return Ok(new
            {
                Date = today.ToString("yyyy-MM-dd"),
                Total = todayAppointments.Count,
                Completed = todayAppointments.Count(a => a.StatusId == 5),
                InProgress = todayAppointments.Count(a => a.StatusId == 4),
                Waiting = todayAppointments.Count(a => a.StatusId == 3),
                Cancelled = todayAppointments.Count(a => a.StatusId == 6),
                Upcoming = todayAppointments.Count(a =>
                    a.StatusId == 1 || a.StatusId == 2)
            });
        }
    }
}

