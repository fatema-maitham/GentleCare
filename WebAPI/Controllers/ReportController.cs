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

        // Helper: shared date range for reports
        private static (DateTime FromDate, DateTime EndDateExclusive) GetDateRange(
            DateTime? from,
            DateTime? to,
            int defaultMonths = 3)
        {
            var fromDate = (from ?? DateTime.Today.AddMonths(-defaultMonths)).Date;
            var endDateExclusive = (to ?? DateTime.Today).Date.AddDays(1);

            return (fromDate, endDateExclusive);
        }

        private static double CalculateRate(int value, int total)
        {
            if (total == 0)
                return 0;

            return Math.Round((double)value / total * 100, 1);
        }

        // GET api/report/appointment-stats
        [HttpGet("appointment-stats")]
        public async Task<IActionResult> GetAppointmentStats(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var query = _context.Appointments
                .Where(a => a.AppointmentDate >= fromDate &&
                            a.AppointmentDate < endDate);

            var total = await query.CountAsync();

            var completed = await query.CountAsync(a => a.StatusId == 5);
            var cancelled = await query.CountAsync(a => a.StatusId == 6);
            var missed = await query.CountAsync(a => a.StatusId == 7);
            var inProgress = await query.CountAsync(a => a.StatusId == 4);
            var checkedIn = await query.CountAsync(a => a.StatusId == 3);
            var confirmed = await query.CountAsync(a => a.StatusId == 2);
            var requested = await query.CountAsync(a => a.StatusId == 1);

            return Ok(new AppointmentStatsDTO
            {
                TotalAppointments = total,
                Completed = completed,
                Cancelled = cancelled,
                Missed = missed,
                InProgress = inProgress,
                CheckedIn = checkedIn,
                Confirmed = confirmed,
                Requested = requested,
                CompletionRate = CalculateRate(completed, total),
                CancellationRate = CalculateRate(cancelled, total),
                MissedRate = CalculateRate(missed, total)
            });
        }

        // GET api/report/appointments-by-period
        [HttpGet("appointments-by-period")]
        public async Task<IActionResult> GetAppointmentsByPeriod(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to, 6);

            var rawData = await _context.Appointments
                .Where(a => a.AppointmentDate >= fromDate &&
                            a.AppointmentDate < endDate)
                .GroupBy(a => new
                {
                    Year = a.AppointmentDate.Year,
                    Month = a.AppointmentDate.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Count(),
                    Completed = g.Count(a => a.StatusId == 5),
                    Cancelled = g.Count(a => a.StatusId == 6),
                    Missed = g.Count(a => a.StatusId == 7)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var result = rawData.Select(x => new AppointmentsByPeriodDTO
            {
                Period = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                Total = x.Total,
                Completed = x.Completed,
                Cancelled = x.Cancelled,
                Missed = x.Missed
            }).ToList();

            return Ok(result);
        }

        // GET api/report/doctor-workload
        [HttpGet("doctor-workload")]
        public async Task<IActionResult> GetDoctorWorkload(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Appointments)
                .ToListAsync();

            var result = doctors.Select(d =>
            {
                var appts = d.Appointments
                    .Where(a => a.AppointmentDate >= fromDate &&
                                a.AppointmentDate < endDate)
                    .ToList();

                var total = appts.Count;
                var completed = appts.Count(a => a.StatusId == 5);
                var cancelled = appts.Count(a => a.StatusId == 6);
                var missed = appts.Count(a => a.StatusId == 7);

                return new DoctorWorkloadDTO
                {
                    DoctorName = d.User.FullName,
                    Specializations = d.DoctorSpecializations
                        .Select(ds => ds.Specialization.Name)
                        .ToList(),
                    TotalAppointments = total,
                    CompletedAppointments = completed,
                    CancelledAppointments = cancelled,
                    MissedAppointments = missed,
                    WorkloadRate = CalculateRate(completed, total)
                };
            })
            .OrderByDescending(x => x.TotalAppointments)
            .ToList();

            return Ok(result);
        }

        // GET api/report/specialization-stats
        [HttpGet("specialization-stats")]
        public async Task<IActionResult> GetSpecializationStats(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

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
                        .Where(a => a.AppointmentDate >= fromDate &&
                                    a.AppointmentDate < endDate)
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
                .OrderByDescending(x => x.TotalAppointments)
                .ToList();

            return Ok(result);
        }

        // GET api/report/patient-stats
        [HttpGet("patient-stats")]
        public async Task<IActionResult> GetPatientStats(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var result = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.AppointmentDate >= fromDate &&
                            a.AppointmentDate < endDate)
                .GroupBy(a => new
                {
                    a.PatientId,
                    PatientName = a.Patient.User.FullName,
                    PatientCreatedAt = a.Patient.CreatedAt
                })
                .Select(g => new
                {
                    g.Key.PatientId,
                    g.Key.PatientName,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(a => a.StatusId == 5),
                    CancelledAppointments = g.Count(a => a.StatusId == 6),
                    MissedAppointments = g.Count(a => a.StatusId == 7),
                    IsNewPatient = g.Key.PatientCreatedAt >= fromDate &&
                                   g.Key.PatientCreatedAt < endDate
                })
                .OrderByDescending(x => x.TotalAppointments)
                .ToListAsync();

            return Ok(result);
        }

        // GET api/report/daily-summary
        [HttpGet("daily-summary")]
        public async Task<IActionResult> GetDailySummary()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todayAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= today &&
                            a.AppointmentDate < tomorrow)
                .ToListAsync();

            return Ok(new
            {
                Date = today.ToString("yyyy-MM-dd"),
                Total = todayAppointments.Count,
                Completed = todayAppointments.Count(a => a.StatusId == 5),
                InProgress = todayAppointments.Count(a => a.StatusId == 4),
                Waiting = todayAppointments.Count(a => a.StatusId == 3),
                Cancelled = todayAppointments.Count(a => a.StatusId == 6),
                Missed = todayAppointments.Count(a => a.StatusId == 7),
                Upcoming = todayAppointments.Count(a => a.StatusId == 2)
            });
        }

        // GET api/report/prescription-stats
        [HttpGet("prescription-stats")]
        public async Task<IActionResult> GetPrescriptionStats(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var query = _context.Prescriptions
                .Include(p => p.VisitRecord)
                    .ThenInclude(v => v.Appointment)
                .Where(p => p.VisitRecord.Appointment.AppointmentDate >= fromDate &&
                            p.VisitRecord.Appointment.AppointmentDate < endDate);

            var total = await query.CountAsync();

            var topMedications = await query
                .GroupBy(p => p.MedicationName)
                .Select(g => new
                {
                    MedicationName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                TotalPrescriptions = total,
                MostPrescribedMedication = topMedications.FirstOrDefault()?.MedicationName ?? "N/A",
                TopMedications = topMedications
            });
        }

        // GET api/report/busiest-hours
        [HttpGet("busiest-hours")]
        public async Task<IActionResult> GetBusiestHours(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var appointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= fromDate &&
                            a.AppointmentDate < endDate)
                .ToListAsync();

            var total = appointments.Count;

            var result = appointments
                .GroupBy(a => a.StartTime.Hour)
                .Select(g => new BusiestHourDTO
                {
                    Hour = g.Key,
                    TimeSlot = $"{g.Key:00}:00 - {g.Key:00}:59",
                    AppointmentCount = g.Count(),
                    AppointmentRate = CalculateRate(g.Count(), total)
                })
                .OrderByDescending(x => x.AppointmentCount)
                .ThenBy(x => x.Hour)
                .ToList();

            return Ok(result);
        }

        // GET api/report/cancellation-reasons
        [HttpGet("cancellation-reasons")]
        public async Task<IActionResult> GetCancellationReasons(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var cancelledAppointments = await _context.Appointments
                .Where(a => a.StatusId == 6 &&
                            a.AppointmentDate >= fromDate &&
                            a.AppointmentDate < endDate)
                .ToListAsync();

            var totalCancelled = cancelledAppointments.Count;

            var result = cancelledAppointments
                .GroupBy(a => string.IsNullOrWhiteSpace(a.CancellationReason)
                    ? "No reason recorded"
                    : a.CancellationReason.Trim())
                .Select(g => new CancellationReasonDTO
                {
                    Reason = g.Key,
                    Count = g.Count(),
                    Rate = CalculateRate(g.Count(), totalCancelled)
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Reason)
                .ToList();

            return Ok(result);
        }

        // GET api/report/missed-appointment-risk
        [HttpGet("missed-appointment-risk")]
        public async Task<IActionResult> GetMissedAppointmentRisk(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.AppointmentDate >= fromDate &&
                            a.AppointmentDate < endDate)
                .ToListAsync();

            var result = appointments
                .Where(a => a.StatusId == 7)
                .GroupBy(a => new
                {
                    a.PatientId,
                    PatientName = a.Patient.User.FullName,
                    a.Patient.CPRNumber
                })
                .Select(g =>
                {
                    var patientTotal = appointments.Count(a => a.PatientId == g.Key.PatientId);
                    var missedCount = g.Count();
                    var missedRate = CalculateRate(missedCount, patientTotal);

                    return new MissedAppointmentRiskDTO
                    {
                        PatientId = g.Key.PatientId,
                        PatientName = g.Key.PatientName,
                        CPRNumber = g.Key.CPRNumber,
                        TotalAppointments = patientTotal,
                        MissedAppointments = missedCount,
                        MissedRate = missedRate,
                        LastMissedDate = g.Max(a => a.AppointmentDate),
                        RiskLevel = missedRate >= 20 ? "High" :
                                    missedRate >= 10 ? "Medium" : "Low"
                    };
                })
                .OrderByDescending(x => x.MissedAppointments)
                .ThenByDescending(x => x.MissedRate)
                .ToList();

            return Ok(result);
        }

        // GET api/report/doctor-leave-impact
        [HttpGet("doctor-leave-impact")]
        public async Task<IActionResult> GetDoctorLeaveImpact(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var appointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= fromDate &&
                            a.AppointmentDate < endDate)
                .ToListAsync();

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Leaves)
                .ToListAsync();

            var result = doctors
                .SelectMany(d => d.Leaves
                    .Where(l => l.StartDate < endDate &&
                                l.EndDate >= fromDate)
                    .Select(l =>
                    {
                        var leaveStart = l.StartDate.Date < fromDate
                            ? fromDate
                            : l.StartDate.Date;

                        var leaveEndExclusive = l.EndDate.Date.AddDays(1) > endDate
                            ? endDate
                            : l.EndDate.Date.AddDays(1);

                        var affected = appointments.Count(a =>
                            a.DoctorId == d.Id &&
                            a.AppointmentDate >= leaveStart &&
                            a.AppointmentDate < leaveEndExclusive &&
                            a.StatusId != 6);

                        return new DoctorLeaveImpactDTO
                        {
                            DoctorName = d.User.FullName,
                            LeaveStartDate = l.StartDate,
                            LeaveEndDate = l.EndDate,
                            Reason = string.IsNullOrWhiteSpace(l.Reason)
                                ? "No reason recorded"
                                : l.Reason,
                            AffectedAppointments = affected
                        };
                    }))
                .OrderByDescending(x => x.AffectedAppointments)
                .ThenBy(x => x.DoctorName)
                .ToList();

            return Ok(result);
        }

        // GET api/report/prescription-volume
        [HttpGet("prescription-volume")]
        public async Task<IActionResult> GetPrescriptionVolume(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var (fromDate, endDate) = GetDateRange(from, to);

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.DoctorSpecializations)
                        .ThenInclude(ds => ds.Specialization)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v.Prescriptions)
                .Where(a => a.AppointmentDate >= fromDate &&
                            a.AppointmentDate < endDate &&
                            a.VisitRecord != null)
                .ToListAsync();

            var result = appointments
                .GroupBy(a => new
                {
                    a.DoctorId,
                    DoctorName = a.Doctor.User.FullName
                })
                .Select(g =>
                {
                    var visitRecords = g.Count();
                    var prescriptionCount = g.Sum(a =>
                        a.VisitRecord?.Prescriptions.Count ?? 0);

                    var specializations = string.Join(", ",
                        g.SelectMany(a => a.Doctor.DoctorSpecializations)
                         .Select(ds => ds.Specialization.Name)
                         .Distinct()
                         .OrderBy(name => name));

                    return new PrescriptionVolumeDTO
                    {
                        DoctorName = g.Key.DoctorName,
                        Specializations = string.IsNullOrWhiteSpace(specializations)
                            ? "No specialization"
                            : specializations,
                        VisitRecords = visitRecords,
                        PrescriptionCount = prescriptionCount,
                        PrescriptionRate = CalculateRate(prescriptionCount, visitRecords)
                    };
                })
                .OrderByDescending(x => x.PrescriptionCount)
                .ThenBy(x => x.DoctorName)
                .ToList();

            return Ok(result);
        }
    }
}