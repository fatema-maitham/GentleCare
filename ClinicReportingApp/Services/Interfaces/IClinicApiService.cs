using ClinicReportingApp.Models;

namespace ClinicReportingApp.Services.Interfaces
{
    public interface IClinicApiService
    {
        // Authenticates a user and returns an access token
        Task<LoginResponse?> LoginAsync(string email, string password);

        // Retrieves overall appointment statistics for a date range
        Task<AppointmentStatsDto> GetAppointmentStatsAsync(
            string token,
            DateTime? from,
            DateTime? to);

        // Returns appointment counts grouped by time period
        Task<List<AppointmentsByPeriodDto>> GetAppointmentsByPeriodAsync(
            string token,
            DateTime? from,
            DateTime? to);

        Task<List<DoctorloadDto>> GetDoctorWorkloadAsync(
            string token,
            DateTime? from,
            DateTime? to);

        Task<List<SpecializationStatsDto>> GetSpecializationStatsAsync(
            string token,
            DateTime? from,
            DateTime? to);

        // Retrieves summary information for the current day
        Task<DailySummaryDto> GetDailySummaryAsync(string token);

        Task<List<PatientStatsDto>> GetPatientStatsAsync(
            string token,
            DateTime? from,
            DateTime? to);

        Task<PrescriptionStatsDto> GetPrescriptionStatsAsync(
            string token,
            DateTime? from,
            DateTime? to);

        Task<List<BusiestHourDto>> GetBusiestHoursAsync(
            string token,
            DateTime? from,
            DateTime? to);

        Task<List<CancellationReasonDto>> GetCancellationReasonsAsync(
            string token,
            DateTime? from,
            DateTime? to);

        Task<List<MissedAppointmentRiskDto>> GetMissedAppointmentRiskAsync(
            string token,
            DateTime? from,
            DateTime? to);


        // Retrieves the impact of doctor leave on appointments
        Task<List<DoctorLeaveImpactDto>> GetDoctorLeaveImpactAsync(
            string token,
            DateTime? from,
            DateTime? to);

        // Returns prescription volume statistics over time
        Task<List<PrescriptionVolumeDto>> GetPrescriptionVolumeAsync(
            string token,
            DateTime? from,
            DateTime? to);
    }
}