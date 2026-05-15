namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager reports page.
    // It shows clinic statistics for a selected date range.
    public class ClinicReportViewModel
    {
        // Report filter dates
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);

        public DateTime ToDate { get; set; } = DateTime.Today;

        // Main appointment statistics
        public int TotalAppointments { get; set; }

        public int RequestedAppointments { get; set; }

        public int ConfirmedAppointments { get; set; }

        public int CheckedInAppointments { get; set; }

        public int InProgressAppointments { get; set; }

        public int CompletedAppointments { get; set; }

        public int CancelledAppointments { get; set; }

        public int MissedAppointments { get; set; }

        // Doctor / workload statistics
        public int TotalDoctors { get; set; }

        public int ActiveDoctors { get; set; }

        public int DoctorsWithAppointments { get; set; }

        public int DoctorsOnLeave { get; set; }

        // Percentages
        public double CompletionRate { get; set; }

        public double CancellationRate { get; set; }

        public double MissedRate { get; set; }

        // Report rows by doctor
        public List<ClinicReportItemViewModel> DoctorReports { get; set; } = new();

        // View helper properties
        public bool HasReportData => TotalAppointments > 0 || DoctorReports.Any();

        public bool HasDoctorReports => DoctorReports.Any();

        public string DateRangeText =>
            FromDate.Date == ToDate.Date
                ? FromDate.ToString("dd MMM yyyy")
                : $"{FromDate:dd MMM yyyy} - {ToDate:dd MMM yyyy}";
    }
}