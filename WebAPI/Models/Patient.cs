namespace WebAPI.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string CPRNumber { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? BloodType { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}