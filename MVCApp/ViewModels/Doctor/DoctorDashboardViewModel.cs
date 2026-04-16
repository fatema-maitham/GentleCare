namespace MVCApp.ViewModels.Doctor
{
    public class DoctorDashboardViewModel
    {
        public string DoctorFullName { get; set; } = string.Empty;

        public int TotalAppointmentsToday { get; set; }
        public int ConfirmedAppointmentsToday { get; set; }
        public int CheckedInAppointmentsToday { get; set; }
        public int InProgressAppointmentsToday { get; set; }
        public int CompletedAppointmentsToday { get; set; }

        public int UpcomingAppointmentsCount { get; set; }
        public int UnreadNotificationsCount { get; set; }
        public int TotalPatientsSeen { get; set; }
    }
}