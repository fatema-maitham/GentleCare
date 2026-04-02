namespace WebAPI.DTOs
{
    public class DoctorResponseDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public List<string> Specializations { get; set; } = new();
        public List<ScheduleDTO> Schedules { get; set; } = new();
    }

    public class ScheduleDTO
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int SlotDurationMinutes { get; set; }
    }

    public class CreateDoctorDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public List<int> SpecializationIds { get; set; } = new();
    }
}