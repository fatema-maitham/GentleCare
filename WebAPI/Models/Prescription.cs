namespace WebAPI.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public int VisitRecordId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public VisitRecord VisitRecord { get; set; } = null!;
    }
}