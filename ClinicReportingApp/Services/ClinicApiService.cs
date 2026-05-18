using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClinicReportingApp.Models;

namespace ClinicReportingApp.Services
{
    public interface IClinicApiService
    {
        Task<LoginResponse?> LoginAsync(string email, string password);
        Task<AppointmentStatsDto> GetAppointmentStatsAsync(string token, DateTime? from, DateTime? to);
        Task<List<AppointmentsByPeriodDto>> GetAppointmentsByPeriodAsync(string token, DateTime? from, DateTime? to); // ← ADD THIS
        Task<List<DoctorloadDto>> GetDoctorWorkloadAsync(string token, DateTime? from, DateTime? to);
        Task<List<SpecializationStatsDto>> GetSpecializationStatsAsync(string token, DateTime? from, DateTime? to);
        Task<DailySummaryDto> GetDailySummaryAsync(string token);
        Task<PatientStatsDto> GetPatientStatsAsync(string token, DateTime? from, DateTime? to);
        Task<PrescriptionStatsDto> GetPrescriptionStatsAsync(string token, DateTime? from, DateTime? to);
    }

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
            Console.WriteLine($">>> BASE URL: {baseUrl}");
                    _http.BaseAddress = new Uri(baseUrl);
        }

        private void SetAuth(string token)
        {
            var cleanToken = token?.Trim();
            Console.WriteLine($">>> FULL TOKEN: {cleanToken}");
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", cleanToken);
        }

        private async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();
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

        // ── Auth ──────────────────────────────────────────────────────────────
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

        // ── GET /api/report/appointment-stats ─────────────────────────────────
        public async Task<AppointmentStatsDto> GetAppointmentStatsAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            var url = BuildUrl("/api/report/appointment-stats", from, to);
            return await GetAsync<AppointmentStatsDto>(url) ?? new AppointmentStatsDto();
        }

        // ── GET /api/report/appointments-by-period ────────────────────────────
        public async Task<List<AppointmentsByPeriodDto>> GetAppointmentsByPeriodAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            var url = BuildUrl("/api/report/appointments-by-period", from, to);
            return await GetAsync<List<AppointmentsByPeriodDto>>(url) ?? new List<AppointmentsByPeriodDto>();
        }

        // ── GET /api/report/doctor-workload ───────────────────────────────────
        public async Task<List<DoctorloadDto>> GetDoctorWorkloadAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            var url = BuildUrl("/api/report/doctor-workload", from, to);
            return await GetAsync<List<DoctorloadDto>>(url) ?? new List<DoctorloadDto>();
        }

        // ── GET /api/report/specialization-stats ──────────────────────────────
        public async Task<List<SpecializationStatsDto>> GetSpecializationStatsAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            var url = BuildUrl("/api/report/specialization-stats", from, to);
            return await GetAsync<List<SpecializationStatsDto>>(url) ?? new List<SpecializationStatsDto>();
        }

        // ── GET /api/report/daily-summary ─────────────────────────────────────
        public async Task<DailySummaryDto> GetDailySummaryAsync(string token)
        {
            SetAuth(token);
            return await GetAsync<DailySummaryDto>("/api/report/daily-summary") ?? new DailySummaryDto();
        }

        // ── GET /api/report/patient-stats ─────────────────────────────────────
        public async Task<PatientStatsDto> GetPatientStatsAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            var url = BuildUrl("/api/report/patient-stats", from, to);
            return await GetAsync<PatientStatsDto>(url) ?? new PatientStatsDto();
        }

        // ── GET /api/report/prescription-stats ────────────────────────────────
        public async Task<PrescriptionStatsDto> GetPrescriptionStatsAsync(string token, DateTime? from, DateTime? to)
        {
            SetAuth(token);
            var url = BuildUrl("/api/report/prescription-stats", from, to);
            return await GetAsync<PrescriptionStatsDto>(url) ?? new PrescriptionStatsDto();
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
