namespace WebAPI.Models
{
    public class VisitRecord : AuditableEntity
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string DoctorNotes { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Treatment { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Appointment Appointment { get; set; } = null!;
        public ICollection<Prescription> Prescriptions { get; set; }
            = new List<Prescription>();
    }
}