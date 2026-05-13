namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Doctor Dashboard page.
    // It contains only summary values needed by the dashboard view.
    public class DoctorDashboardViewModel
    {
        // Doctor name displayed in the dashboard welcome message.
        public string DoctorFullName { get; set; } = string.Empty;

        // Total number of appointments assigned to this doctor today.
        public int TotalAppointmentsToday { get; set; }

        // Today's appointments grouped by appointment workflow status.
        public int ConfirmedAppointmentsToday { get; set; }
        public int CheckedInAppointmentsToday { get; set; }
        public int InProgressAppointmentsToday { get; set; }
        public int CompletedAppointmentsToday { get; set; }

        // Future appointments that are not completed, cancelled, or missed.
        public int UpcomingAppointmentsCount { get; set; }

        // Number of unread notifications for the logged-in doctor.
        public int UnreadNotificationsCount { get; set; }

        // Number of unique patients this doctor has completed appointments with.
        public int TotalPatientsSeen { get; set; }

        // Helpful computed value for views if you want to show remaining appointments today.
        public int RemainingAppointmentsToday =>
            TotalAppointmentsToday - CompletedAppointmentsToday;
    }
}