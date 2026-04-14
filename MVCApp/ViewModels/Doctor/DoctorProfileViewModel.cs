namespace MVCApp.ViewModels.Doctor
{
    public class DoctorProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public List<string> Specializations { get; set; } = new();
        public List<DoctorScheduleItemViewModel> Schedules { get; set; } = new();
        public List<DoctorLeaveItemViewModel> Leaves { get; set; } = new();
    }

    public class DoctorScheduleItemViewModel
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int SlotDurationMinutes { get; set; }
    }

    public class DoctorLeaveItemViewModel
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
