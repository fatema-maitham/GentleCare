namespace WebAPI.Models
{
    public class Appointment : AuditableEntity
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int StatusId { get; set; } 
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime? UpdatedAt { get; set; }

        
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public AppointmentStatusLookup Status { get; set; } = null!;
        public VisitRecord? VisitRecord { get; set; }
    }
}