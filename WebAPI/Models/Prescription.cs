namespace WebAPI.Models
{
    public class Prescription : AuditableEntity
    {
        public int Id { get; set; }
        public int VisitRecordId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public VisitRecord VisitRecord { get; set; } = null!;
    }
}