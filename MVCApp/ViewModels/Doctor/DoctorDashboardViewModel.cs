namespace MVCApp.ViewModels.Doctor
{
    // ViewModel for the Doctor Dashboard page.
    // It contains selected-day statistics, monthly calendar data, and selected-day appointments.
    public class DoctorDashboardViewModel
    {
        // Doctor name displayed in the dashboard header.
        public string DoctorFullName { get; set; } = string.Empty;

        // The date selected in the dashboard calendar.
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        // Total appointments on the selected date.
        public int TotalAppointmentsForSelectedDate { get; set; }

        // Number of confirmed appointments on the selected date.
        public int ConfirmedAppointmentsForSelectedDate { get; set; }

        // Number of checked-in appointments on the selected date.
        public int CheckedInAppointmentsForSelectedDate { get; set; }

        // Number of in-progress appointments on the selected date.
        public int InProgressAppointmentsForSelectedDate { get; set; }

        // Number of completed appointments on the selected date.
        public int CompletedAppointmentsForSelectedDate { get; set; }

        // Number of future active appointments for this doctor.
        public int UpcomingAppointmentsCount { get; set; }

        // Number of unread notifications for this doctor.
        public int UnreadNotificationsCount { get; set; }

        // Number of unique patients with completed appointments.
        public int TotalPatientsSeen { get; set; }

        // Calendar month label, for example MAY.
        public string CalendarMonthLabel => SelectedDate.ToString("MMM").ToUpper();

        // Calendar year, for example 2026.
        public int CalendarYear => SelectedDate.Year;

        // Date used for the previous-month navigation button.
        public DateTime PreviousMonthDate =>
            new DateTime(SelectedDate.Year, SelectedDate.Month, 1).AddMonths(-1);

        // Date used for the next-month navigation button.
        public DateTime NextMonthDate =>
            new DateTime(SelectedDate.Year, SelectedDate.Month, 1).AddMonths(1);

        // Calendar day boxes shown in the dashboard calendar.
        public List<DoctorDashboardCalendarDayViewModel> CalendarDays { get; set; } = new();

        // Appointment list for the selected calendar day.
        public List<DoctorDashboardAppointmentItemViewModel> SelectedDayAppointments { get; set; } = new();

        // Used by the view to show an empty message when selected day has no appointments.
        public bool HasSelectedDayAppointments => SelectedDayAppointments.Any();
    }

    // One day box inside the dashboard calendar.
    public class DoctorDashboardCalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public int DayNumber { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsSelected { get; set; }
        public bool HasAppointments { get; set; }
        public int AppointmentCount { get; set; }
    }

    // One appointment item shown under the selected day in the dashboard.
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
