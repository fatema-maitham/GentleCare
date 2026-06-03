using ClinicReportingApp.Models;
using ClinicReportingApp.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ClinicReportingApp.Services
{
    public class ClinicApiService : IClinicApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ClinicApiService> _logger;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ClinicApiService(HttpClient http, ILogger<ClinicApiService> logger, IConfiguration config)
        {
            _http = http;
            _logger = logger;
            var baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7117";
            _http.BaseAddress = new Uri(baseUrl);
        }

        private void SetAuth(string token)
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                var body     = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("GET {Url} → {Status}", url, response.StatusCode);
                if (response.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<T>(body, _json);
                _logger.LogWarning("API error: {Body}", body);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("API call failed: {Message}", ex.Message);
                return default;
            }
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new { email, password };
                var json    = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("/api/auth/login", content);
                var body     = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Login → {Status}: {Body}", response.StatusCode, body);
                if (response.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<LoginResponse>(body, _json);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<AppointmentStatsDto> GetAppointmentStatsAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            return await GetAsync<AppointmentStatsDto>(BuildUrl("/api/report/appointment-stats", from, to)) ?? new AppointmentStatsDto();
        }

        public async Task<List<AppointmentsByPeriodDto>> GetAppointmentsByPeriodAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            return await GetAsync<List<AppointmentsByPeriodDto>>(BuildUrl("/api/report/appointments-by-period", from, to)) ?? new();
        }

        public async Task<List<DoctorloadDto>> GetDoctorWorkloadAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            return await GetAsync<List<DoctorloadDto>>(BuildUrl("/api/report/doctor-workload", from, to)) ?? new();
        }

        public async Task<List<SpecializationStatsDto>> GetSpecializationStatsAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            return await GetAsync<List<SpecializationStatsDto>>(BuildUrl("/api/report/specialization-stats", from, to)) ?? new();
        }

        public async Task<DailySummaryDto> GetDailySummaryAsync(string token)
        {
            SetAuth(token);
            return await GetAsync<DailySummaryDto>("/api/report/daily-summary") ?? new DailySummaryDto();
        }

        public async Task<List<PatientStatsDto>> GetPatientStatsAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            return await GetAsync<List<PatientStatsDto>>(BuildUrl("/api/report/patient-stats", from, to)) ?? new List<PatientStatsDto>();
        }

        public async Task<PrescriptionStatsDto> GetPrescriptionStatsAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            return await GetAsync<PrescriptionStatsDto>(BuildUrl("/api/report/prescription-stats", from, to)) ?? new PrescriptionStatsDto();
        }
        // GET /api/report/busiest-hours
        public async Task<List<BusiestHourDto>> GetBusiestHoursAsync(
            string token,
            DateTime? from,
            DateTime? to)
        {
            SetAuth(token);

            return await GetAsync<List<BusiestHourDto>>(
                BuildUrl("/api/report/busiest-hours", from, to))
                ?? new();
        }

        // GET /api/report/cancellation-reasons
        public async Task<List<CancellationReasonDto>> GetCancellationReasonsAsync(
            string token,
            DateTime? from,
            DateTime? to)
        {
            SetAuth(token);

            return await GetAsync<List<CancellationReasonDto>>(
                BuildUrl("/api/report/cancellation-reasons", from, to))
                ?? new();
        }

        // GET /api/report/missed-appointment-risk
        public async Task<List<MissedAppointmentRiskDto>> GetMissedAppointmentRiskAsync(
            string token,
            DateTime? from,
            DateTime? to)
        {
            SetAuth(token);

            return await GetAsync<List<MissedAppointmentRiskDto>>(
                BuildUrl("/api/report/missed-appointment-risk", from, to))
                ?? new();
        }

        // GET /api/report/doctor-leave-impact
        public async Task<List<DoctorLeaveImpactDto>> GetDoctorLeaveImpactAsync(
            string token,
            DateTime? from,
            DateTime? to)
        {
            SetAuth(token);

            return await GetAsync<List<DoctorLeaveImpactDto>>(
                BuildUrl("/api/report/doctor-leave-impact", from, to))
                ?? new();
        }

        // GET /api/report/prescription-volume
        public async Task<List<PrescriptionVolumeDto>> GetPrescriptionVolumeAsync(
            string token,
            DateTime? from,
            DateTime? to)
        {
            SetAuth(token);

            return await GetAsync<List<PrescriptionVolumeDto>>(
                BuildUrl("/api/report/prescription-volume", from, to))
                ?? new();
        }
        private static string BuildUrl(string path, DateTime? from, DateTime? to)
        {
            var query = new List<string>();
            if (from.HasValue) query.Add($"from={from.Value:yyyy-MM-dd}");
            if (to.HasValue)   query.Add($"to={to.Value:yyyy-MM-dd}");
            return query.Count > 0 ? $"{path}?{string.Join("&", query)}" : path;
        }
    }
}
