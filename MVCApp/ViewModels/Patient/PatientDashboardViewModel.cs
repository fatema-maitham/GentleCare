namespace MVCApp.ViewModels.Patient
{
    public class PatientDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public int TotalAppointmentsCount { get; set; }

        public int UpcomingAppointmentsCount { get; set; }

        public int CompletedVisitsCount { get; set; }

        public int UnreadNotificationsCount { get; set; }

        public PatientAppointmentsViewModel? NextAppointment { get; set; }

        public PatientHistoryViewModel? LatestVisit { get; set; }
    }
}