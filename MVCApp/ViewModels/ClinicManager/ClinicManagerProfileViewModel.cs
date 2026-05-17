namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager Profile page.
    public class ClinicManagerProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? ProfilePicture { get; set; }
    }
}