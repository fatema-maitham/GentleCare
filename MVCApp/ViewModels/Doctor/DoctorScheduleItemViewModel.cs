namespace MVCApp.ViewModels.Doctor
{
    // Represents one working schedule row for the doctor's schedule page.
    public class DoctorScheduleItemViewModel
    {
        // Schedule primary key.
        public int DoctorScheduleId { get; set; }

        // Day the doctor is available, for example Monday or Tuesday.
        public DayOfWeek DayOfWeek { get; set; }

        // Doctor working time for this day.
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Appointment slot duration in minutes.
        public int SlotDurationMinutes { get; set; }

        // Display helpers for the view.
        public string DayDisplay => DayOfWeek.ToString();
        public string TimeDisplay => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";
        public string SlotDurationDisplay => $"{SlotDurationMinutes} minutes";

        // Used by the view to show a warning if the schedule time is not valid.
        public bool IsValidTimeRange => EndTime > StartTime;
    }
}