namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager reports page.
    public class ClinicReportViewModel
    {
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);

        public DateTime ToDate { get; set; } = DateTime.Today;

        public int TotalAppointments { get; set; }

        public int RequestedAppointments { get; set; }

        public int ConfirmedAppointments { get; set; }

        public int CheckedInAppointments { get; set; }

        public int InProgressAppointments { get; set; }

        public int CompletedAppointments { get; set; }

        public int CancelledAppointments { get; set; }

        public int MissedAppointments { get; set; }

        public int TotalDoctors { get; set; }

        public int ActiveDoctors { get; set; }

        public int DoctorsWithAppointments { get; set; }

        public int DoctorsOnLeave { get; set; }

        public double CompletionRate { get; set; }

        public double CancellationRate { get; set; }

        public double MissedRate { get; set; }

        public string BusiestDoctorName { get; set; } = "None";

        public string BusiestSpecializationName { get; set; } = "None";

        public string BusiestHourText { get; set; } = "None";

        public List<ClinicReportItemViewModel> DoctorReports { get; set; } = new();

        public List<SpecializationDemandReportViewModel> SpecializationDemandReports { get; set; } = new();

        public List<BusiestHourReportViewModel> BusiestHourReports { get; set; } = new();

        public List<DoctorLeaveImpactReportViewModel> DoctorLeaveImpactReports { get; set; } = new();

        public List<MissedAppointmentRiskReportViewModel> MissedAppointmentRiskReports { get; set; } = new();

        public List<CancellationReasonReportViewModel> CancellationReasonReports { get; set; } = new();

        public List<PrescriptionVolumeReportViewModel> PrescriptionVolumeReports { get; set; } = new();

        public bool HasReportData => TotalAppointments > 0 || DoctorReports.Any();

        public bool HasDoctorReports => DoctorReports.Any();

        public bool HasSpecializationDemandReports => SpecializationDemandReports.Any();

        public bool HasBusiestHourReports => BusiestHourReports.Any();

        public bool HasDoctorLeaveImpactReports => DoctorLeaveImpactReports.Any();

        public bool HasMissedAppointmentRiskReports => MissedAppointmentRiskReports.Any();

        public bool HasCancellationReasonReports => CancellationReasonReports.Any();

        public bool HasPrescriptionVolumeReports => PrescriptionVolumeReports.Any();

        public string DateRangeText =>
            FromDate.Date == ToDate.Date
                ? FromDate.ToString("dd MMM yyyy")
                : $"{FromDate:dd MMM yyyy} - {ToDate:dd MMM yyyy}";
    }

    public class SpecializationDemandReportViewModel
    {
        public string SpecializationName { get; set; } = string.Empty;

        public int AppointmentCount { get; set; }

        public int DoctorCount { get; set; }

        public double DemandRate { get; set; }
    }

    public class BusiestHourReportViewModel
    {
        public int Hour { get; set; }

        public string TimeSlot { get; set; } = string.Empty;

        public int AppointmentCount { get; set; }

        public double AppointmentRate { get; set; }
    }

    public class DoctorLeaveImpactReportViewModel
    {
        public string DoctorName { get; set; } = string.Empty;

        public DateTime LeaveStartDate { get; set; }

        public DateTime LeaveEndDate { get; set; }

        public string Reason { get; set; } = string.Empty;

        public int AffectedAppointments { get; set; }

        public string LeavePeriodText =>
            LeaveStartDate.Date == LeaveEndDate.Date
                ? LeaveStartDate.ToString("dd MMM yyyy")
                : $"{LeaveStartDate:dd MMM yyyy} - {LeaveEndDate:dd MMM yyyy}";
    }

    public class MissedAppointmentRiskReportViewModel
    {
        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string CPRNumber { get; set; } = string.Empty;

        public int TotalAppointments { get; set; }

        public int MissedAppointments { get; set; }

        public DateTime? LastMissedDate { get; set; }

        public double MissedRate { get; set; }

        public string RiskLevel
        {
            get
            {
                if (MissedAppointments >= 3)
                {
                    return "High";
                }

                if (MissedAppointments >= 2)
                {
                    return "Medium";
                }

                return "Low";
            }
        }

        public string LastMissedDateDisplay =>
            LastMissedDate.HasValue
                ? LastMissedDate.Value.ToString("dd MMM yyyy")
                : "-";
    }

    public class CancellationReasonReportViewModel
    {
        public string Reason { get; set; } = string.Empty;

        public int Count { get; set; }

        public double Rate { get; set; }
    }

    public class PrescriptionVolumeReportViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string Specializations { get; set; } = string.Empty;

        public int VisitRecords { get; set; }

        public int PrescriptionCount { get; set; }

        public double PrescriptionRate { get; set; }
    }
}