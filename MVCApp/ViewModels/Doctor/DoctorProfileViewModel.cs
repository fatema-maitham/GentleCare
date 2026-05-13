nnamespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Doctor Profile page.
    // It shows the logged-in doctor's profile details and assigned specializations.
    public class DoctorProfileViewModel
    {
        // Doctor primary key.
        public int DoctorId { get; set; }

        // Basic profile information.
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Bio { get; set; }

        // Specializations assigned by the clinic manager.
        public List<string> Specializations { get; set; } = new();

        // Display helper for the view.
        public bool HasSpecializations => Specializations.Any();
    }
}