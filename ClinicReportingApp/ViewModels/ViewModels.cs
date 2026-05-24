using ClinicReportingApp.Models;

namespace ClinicReportingApp.ViewModels
{
    public class DashboardViewModel
    {
        public List<BusiestHourDto> BusiestHours { get; set; } = new();
        public AppointmentStatsDto AppointmentStats  { get; set; } = new();
        public List<DoctorloadDto> DoctorWorkload { get; set; } = new();
        public DailySummaryDto DailySummary          { get; set; } = new();
        public List<SpecializationStatsDto> SpecStats { get; set; } = new();
        public List<PatientStatsDto> PatientActivity { get; set; } = new();
        public PrescriptionStatsDto PrescriptionStats { get; set; } = new();
        public string ManagerName  { get; set; } = "";
        public string PeriodLabel  { get; set; } = "";
        public int NewPatients =>                                   
                    PatientActivity.Count(p => p.IsNewPatient);

    }

    public class AppointmentReportViewModel
    {
        public AppointmentStatsDto Stats  { get; set; } = new();
        public DailySummaryDto DailySummary { get; set; } = new();
        public DateRangeFilter Filter     { get; set; } = new();
        public List<AppointmentsByPeriodDto> ByPeriod { get; set; } = new();
        public List<SpecializationStatsDto> SpecializationStats { get; set; } = new();
        public List<CancellationReasonDto> CancellationReasons { get; set; } = new();

    }

    public class DoctorReportViewModel
    {
        public List<DoctorloadDto>  DoctorWorkload      { get; set; } = new();
        public List<SpecializationStatsDto> SpecializationStats { get; set; } = new();
        public DateRangeFilter Filter { get; set; } = new();
        public List<DoctorLeaveImpactDto> DoctorLeaveImpact { get; set; } = new();


    }

    public class SpecializationReportViewModel
    {
        public List<SpecializationStatsDto> Stats { get; set; } = new();
        public DateRangeFilter Filter { get; set; } = new();
    }

        public class PatientReportViewModel
    {
        // Report Data
        public List<PatientStatsDto> PatientActivity { get; set; } = new();

        // Filters
        public DateRangeFilter Filter { get; set; } = new();

        // Calculated summary cards not Calculated in WebAPI
        public int TotalPatients => PatientActivity.Count;
        public int NewPatients =>
         PatientActivity.Count(p => p.IsNewPatient);
        public int ActivePatients =>
            PatientActivity.Count(p => p.TotalAppointments > 0);

        public double AvgAppointmentsPerPatient =>
            PatientActivity.Count == 0
                ? 0
                : PatientActivity.Average(p => p.TotalAppointments);
    }

    public class PrescriptionReportViewModel
    {
        public PrescriptionStatsDto Stats { get; set; } = new();
        public DateRangeFilter Filter     { get; set; } = new();
        public List<PrescriptionVolumeDto> PrescriptionVolume { get; set; } = new();
    }
}
