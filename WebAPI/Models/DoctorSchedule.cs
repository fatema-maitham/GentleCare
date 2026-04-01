namespace WebAPI.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
        public Doctor Doctor { get; set; } = null!;
    }
}