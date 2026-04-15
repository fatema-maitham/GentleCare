namespace MVCApp.ViewModels.ClinicManager
{
    public class DoctorDetailsViewModel
    {
        public int DoctorId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsActive { get; set; }

        public List<string> Specializations { get; set; } = new();
        public List<DoctorScheduleItemViewModel> Schedules { get; set; } = new();
        public List<DoctorLeaveItemViewModel> Leaves { get; set; } = new();

        public int UpcomingAppointmentsCount { get; set; }
    }
}
