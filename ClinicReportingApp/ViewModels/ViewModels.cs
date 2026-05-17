using ClinicReportingApp.Models;

namespace ClinicReportingApp.ViewModels
{
    public class DashboardViewModel
    {
        public AppointmentStatsDto AppointmentStats  { get; set; } = new();
        public List<DoctorloadDto> DoctorWorkload { get; set; } = new();
        public DailySummaryDto DailySummary          { get; set; } = new();
        public List<SpecializationStatsDto> SpecStats { get; set; } = new();
        public PatientStatsDto PatientActivity { get; set; } = new();
        public PrescriptionStatsDto PrescriptionStats { get; set; } = new();
        public string ManagerName  { get; set; } = "";
        public string PeriodLabel  { get; set; } = "";
    }

    public class AppointmentReportViewModel
    {
        public AppointmentStatsDto Stats  { get; set; } = new();
        public DailySummaryDto DailySummary { get; set; } = new();
        public DateRangeFilter Filter     { get; set; } = new();
        public List<AppointmentsByPeriodDto> ByPeriod { get; set; } = new();
    }

    public class DoctorReportViewModel
    {
        public List<DoctorloadDto>  DoctorWorkload      { get; set; } = new();
        public List<SpecializationStatsDto> SpecializationStats { get; set; } = new();
        public DateRangeFilter Filter { get; set; } = new();
    }

    public class SpecializationReportViewModel
    {
        public List<SpecializationStatsDto> Stats { get; set; } = new();
        public DateRangeFilter Filter { get; set; } = new();
    }

    public class PatientReportViewModel
    {
        public PatientStatsDto Stats  { get; set; } = new();
        public DateRangeFilter Filter { get; set; } = new();
        public PatientStatsDto PatientActivity { get; set; } = new();
    }

    public class PrescriptionReportViewModel
    {
        public PrescriptionStatsDto Stats { get; set; } = new();
        public DateRangeFilter Filter     { get; set; } = new();
    }
}
