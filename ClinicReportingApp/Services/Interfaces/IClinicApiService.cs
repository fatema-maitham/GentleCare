using ClinicReportingApp.Models;

namespace ClinicReportingApp.Services.Interfaces
{
    public interface IClinicApiService
    {
        Task<LoginResponse?> LoginAsync(string email, string password);

        Task<AppointmentStatsDto> GetAppointmentStatsAsync(
            string token,
            DateTime? from,
            DateTime? to);

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

        Task<List<DoctorLeaveImpactDto>> GetDoctorLeaveImpactAsync(
            string token,
            DateTime? from,
            DateTime? to);

        Task<List<PrescriptionVolumeDto>> GetPrescriptionVolumeAsync(
            string token,
            DateTime? from,
            DateTime? to);
    }
}