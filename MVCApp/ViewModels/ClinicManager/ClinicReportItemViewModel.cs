namespace MVCApp.ViewModels.ClinicManager
{
    // One doctor workload row shown in the Clinic Manager reports page.
    public class ClinicReportItemViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string DoctorEmail { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        public int TotalAppointments { get; set; }

        public int CompletedAppointments { get; set; }

        public int CancelledAppointments { get; set; }

        public int MissedAppointments { get; set; }

        public int RemainingAppointments { get; set; }

        public double CompletionRate { get; set; }

        public double CancellationRate { get; set; }

        public double MissedRate { get; set; }

        public string WorkloadLevel
        {
            get
            {
                if (TotalAppointments >= 20)
                {
                    return "High";
                }

                if (TotalAppointments >= 10)
                {
                    return "Medium";
                }

                return "Low";
            }
        }
    }
}