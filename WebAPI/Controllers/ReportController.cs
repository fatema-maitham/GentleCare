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
    [Authorize(Roles = "ClinicManager")]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

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

            var completed = await query.CountAsync(a =>
                a.Status == AppointmentStatus.Completed);
            var cancelled = await query.CountAsync(a =>
                a.Status == AppointmentStatus.Cancelled);
            var missed = await query.CountAsync(a =>
                a.Status == AppointmentStatus.Missed);
            var pending = await query.CountAsync(a =>
                a.Status == AppointmentStatus.Requested ||
                a.Status == AppointmentStatus.Confirmed ||
                a.Status == AppointmentStatus.CheckedIn ||
                a.Status == AppointmentStatus.InProgress);

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
                var appointments = d.Appointments.AsQueryable();

                if (from.HasValue)
                    appointments = appointments
                        .Where(a => a.AppointmentDate >= from.Value);

                if (to.HasValue)
                    appointments = appointments
                        .Where(a => a.AppointmentDate <= to.Value);

                var apptList = appointments.ToList();

                return new DoctorWorkloadDTO
                {
                    DoctorName = d.User.FullName,
                    Specializations = d.DoctorSpecializations
                        .Select(ds => ds.Specialization.Name)
                        .ToList(),
                    TotalAppointments = apptList.Count,
                    CompletedAppointments = apptList.Count(a =>
                        a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = apptList.Count(a =>
                        a.Status == AppointmentStatus.Cancelled),
                    MissedAppointments = apptList.Count(a =>
                        a.Status == AppointmentStatus.Missed)
                };
            }).ToList();

            return Ok(result);
        }

        [HttpGet("specialization-stats")]
        public async Task<IActionResult> GetSpecializationStats(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var query = _context.Appointments
                .Include(a => a.Specialization)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(a => a.AppointmentDate >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.AppointmentDate <= to.Value);

            var result = await query
                .GroupBy(a => a.Specialization.Name)
                .Select(g => new SpecializationStatsDTO
                {
                    SpecializationName = g.Key,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(a =>
                        a.Status == AppointmentStatus.Completed)
                })
                .ToListAsync();

            return Ok(result);
        }


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
                Completed = todayAppointments.Count(a =>
                    a.Status == AppointmentStatus.Completed),
                InProgress = todayAppointments.Count(a =>
                    a.Status == AppointmentStatus.InProgress),
                Waiting = todayAppointments.Count(a =>
                    a.Status == AppointmentStatus.CheckedIn),
                Cancelled = todayAppointments.Count(a =>
                    a.Status == AppointmentStatus.Cancelled),
                Upcoming = todayAppointments.Count(a =>
                    a.Status == AppointmentStatus.Confirmed ||
                    a.Status == AppointmentStatus.Requested)
            });
        }
    }
}