namespace MVCApp.ViewModels.Doctor
{
    public class DoctorDashboardViewModel
    {
        public string DoctorFullName { get; set; } = string.Empty;

        public DateTime SelectedDate { get; set; } = DateTime.Today;

        public int TotalAppointmentsForSelectedDate { get; set; }

        public int ConfirmedAppointmentsForSelectedDate { get; set; }

        public int CheckedInAppointmentsForSelectedDate { get; set; }

        public int InProgressAppointmentsForSelectedDate { get; set; }

        public int CompletedAppointmentsForSelectedDate { get; set; }

        public int UpcomingAppointmentsCount { get; set; }

        public int UnreadNotificationsCount { get; set; }

        public int TotalPatientsSeen { get; set; }

        public string CalendarMonthLabel => SelectedDate.ToString("MMM").ToUpper();

        public int CalendarYear => SelectedDate.Year;

        public DateTime PreviousMonthDate =>
            new DateTime(SelectedDate.Year, SelectedDate.Month, 1).AddMonths(-1);

        public DateTime NextMonthDate =>
            new DateTime(SelectedDate.Year, SelectedDate.Month, 1).AddMonths(1);

        public List<DoctorDashboardCalendarDayViewModel> CalendarDays { get; set; } = new();

        public List<DoctorDashboardAppointmentItemViewModel> SelectedDayAppointments { get; set; } = new();

        public bool HasSelectedDayAppointments => SelectedDayAppointments.Any();
    }

    public class DoctorDashboardCalendarDayViewModel
    {
        public DateTime Date { get; set; }

        public int DayNumber { get; set; }

        public bool IsCurrentMonth { get; set; }

        public bool IsSelected { get; set; }

        public bool HasAppointments { get; set; }

        public int AppointmentCount { get; set; }
    }

    public class DoctorDashboardAppointmentItemViewModel
    {
        public int AppointmentId { get; set; }

        public string PatientFullName { get; set; } = string.Empty;

        public string PatientReferenceNumber { get; set; } = string.Empty;

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public string TimeDisplay => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";
    }
}