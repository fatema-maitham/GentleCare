using ClinicReportingApp.Services;
using ClinicReportingApp.Services.Interfaces;
using ClinicReportingApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ClinicReportingApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IClinicApiService _api;
        private readonly ITokenService _tokenService;

        public DashboardController(IClinicApiService api, ITokenService tokenService)
        {
            _api = api;
            _tokenService = tokenService;
        }

        public async Task<IActionResult> Index()
        {
            if (!_tokenService.IsAuthenticated(HttpContext))
                return RedirectToAction("Login", "Auth");

            var token = _tokenService.GetToken(HttpContext)!;

            // Default: last 30 days /like a filter
            var from = DateTime.Today.AddDays(-30);
            var to = DateTime.Today;

            var stats         = await _api.GetAppointmentStatsAsync(token, from, to);
            var doctorStats   = await _api.GetDoctorWorkloadAsync(token, from, to);
            var patientStats  = await _api.GetPatientStatsAsync(token, from, to);
            var prescriptions = await _api.GetPrescriptionStatsAsync(token, from, to);
            var specs = await _api.GetSpecializationStatsAsync(token, from, to);
            var daily = await _api.GetDailySummaryAsync(token);
            var busiestHours = await _api.GetBusiestHoursAsync(token, from, to);

            var vm = new DashboardViewModel
            {
                BusiestHours = busiestHours,
                AppointmentStats = stats,
                DoctorWorkload = doctorStats,
                PatientActivity = patientStats,
                PrescriptionStats = prescriptions,
                SpecStats = specs,
                DailySummary = daily,
                ManagerName = TokenService.GetUserName(HttpContext) ?? "Clinic Manager",
                PeriodLabel = $"{from.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)} – " +
                              $"{to.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}"
            };

            return View(vm);
        }
    }
}
