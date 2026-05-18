namespace WebAPI.Models
{
    public class Doctor : AuditableEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Bio { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime? UpdatedAt { get; set; }

   
        public ApplicationUser User { get; set; } = null!;
        public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; }
            = new List<DoctorSpecialization>();
        public ICollection<DoctorSchedule> Schedules { get; set; }
            = new List<DoctorSchedule>();
        public ICollection<DoctorLeave> Leaves { get; set; }
            = new List<DoctorLeave>();
        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}