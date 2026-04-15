namespace MVCApp.ViewModels.Doctor
{
    public class DoctorProfileViewModel
    {
        public int DoctorId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;
        public string? Bio { get; set; }

        public List<string> Specializations { get; set; } = new();
    }
}