namespace MVCApp.ViewModels.ClinicManager
{
    // One schedule row displayed in the Clinic Manager doctor schedule page.
    public class DoctorScheduleItemViewModel
    {
        public int DoctorScheduleId { get; set; }

        public int DoctorId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int SlotDurationMinutes { get; set; }

        // View helper text
        public string DayName => DayOfWeek.ToString();

        public string TimeRangeText =>
            $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";

        public string ScheduleText =>
            $"{DayName}, {TimeRangeText}";

        public double WorkingHours =>
            (EndTime.ToTimeSpan() - StartTime.ToTimeSpan()).TotalHours;
    }
}