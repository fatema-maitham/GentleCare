using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Appointment
{
    public class CreateVisitRecordViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Doctor Notes")]
        public string DoctorNotes { get; set; } = string.Empty;

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        public string? Treatment { get; set; }
    }
}
