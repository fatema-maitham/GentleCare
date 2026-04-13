namespace WebAPI.DTOs
{
    public class CreatePatientDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string CPRNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? BloodType { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
    }

    public class UpdatePatientDTO
    {
        public string? BloodType { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
    }

    public class PatientResponseDTO
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CPRNumber { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? BloodType { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}