namespace MVCApp.ViewModels.ClinicManager
{
    public class DoctorScheduleItemViewModel
    {
        public int DoctorScheduleId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public int SlotDurationMinutes { get; set; }
    }
}
