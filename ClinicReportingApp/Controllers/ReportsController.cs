using ClinicReportingApp.Models;
using ClinicReportingApp.Services;
using ClinicReportingApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicReportingApp.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IClinicApiService _api;
        private readonly ITokenService _tokenService;

        public ReportsController(IClinicApiService api, ITokenService tokenService)
        {
            _api = api;
            _tokenService = tokenService;
        }

        private (bool ok, string token, IActionResult? redirect) Guard()
        {
            if (!_tokenService.IsAuthenticated(HttpContext))
                return (false, "", RedirectToAction("Login", "Auth"));
            return (true, _tokenService.GetToken(HttpContext)!, null);
        }

        // ── Appointments Report ───────────────────────────────────────────────
        public async Task<IActionResult> Appointments(DateTime? from, DateTime? to)
        {
            var (ok, token, redirect) = Guard();
            if (!ok) return redirect!;

            from ??= DateTime.Today.AddMonths(-3);
            to   ??= DateTime.Today;

            var stats = await _api.GetAppointmentStatsAsync(token, from, to);
            var daily = await _api.GetDailySummaryAsync(token);
            var byPeriod = await _api.GetAppointmentsByPeriodAsync(token, from, to);

            return View(new AppointmentReportViewModel
            {
                Stats       = stats,
                DailySummary = daily,
                Filter      = new DateRangeFilter { From = from, To = to },
                ByPeriod = byPeriod,
            });
        }


        // ── Doctor Load Report ─────────────────────────────────────────
        public async Task<IActionResult> DoctorWorkload(DateTime? from, DateTime? to)
        {
            var (ok, token, redirect) = Guard();
            if (!ok) return redirect!;

            from ??= DateTime.Today.AddMonths(-3);
            to   ??= DateTime.Today;

            var doctors = await _api.GetDoctorWorkloadAsync(token, from, to);
            var specs   = await _api.GetSpecializationStatsAsync(token, from, to);

            return View(new DoctorReportViewModel
            {
                DoctorWorkload      = doctors,
                SpecializationStats = specs,
                Filter = new DateRangeFilter { From = from, To = to }
            });
        }

        // ── Specialization Report ─────────────────────────────────────────────
        public async Task<IActionResult> Specializations(DateTime? from, DateTime? to)
        {
            var (ok, token, redirect) = Guard();
            if (!ok) return redirect!;

            from ??= DateTime.Today.AddMonths(-3);
            to   ??= DateTime.Today;

            var specs = await _api.GetSpecializationStatsAsync(token, from, to);

            return View(new SpecializationReportViewModel
            {
                Stats  = specs,
                Filter = new DateRangeFilter { From = from, To = to }
            });
        }

        // ── Patients Report ───────────────────────────────────────────────────
        public async Task<IActionResult> Patients(DateTime? from, DateTime? to)
        {
            var (ok, token, redirect) = Guard();
            if (!ok) return redirect!;

            from ??= DateTime.Today.AddMonths(-3);
            to   ??= DateTime.Today;

            var stats = await _api.GetPatientStatsAsync(token, from, to);

            return View(new PatientReportViewModel
            {
                Stats  = stats,
                Filter = new DateRangeFilter { From = from, To = to }
            });
        }

        // ── Prescriptions Report ──────────────────────────────────────────────
        public async Task<IActionResult> Prescriptions(DateTime? from, DateTime? to)
        {
            var (ok, token, redirect) = Guard();
            if (!ok) return redirect!;

            from ??= DateTime.Today.AddMonths(-3);
            to   ??= DateTime.Today;

            var stats = await _api.GetPrescriptionStatsAsync(token, from, to);

            return View(new PrescriptionReportViewModel
            {
                Stats  = stats,
                Filter = new DateRangeFilter { From = from, To = to }
            });
        }
    }
}
