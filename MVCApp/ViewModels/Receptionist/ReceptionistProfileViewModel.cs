namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        public string? ProfilePicture { get; set; }
    }
}