namespace MVCApp.ViewModels.ClinicManager
{
    public class DoctorListItemViewModel
    {
        public int DoctorId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Bio { get; set; }

        public List<string> Specializations { get; set; } = new();
        public int UpcomingAppointmentsCount { get; set; }
    }
}
