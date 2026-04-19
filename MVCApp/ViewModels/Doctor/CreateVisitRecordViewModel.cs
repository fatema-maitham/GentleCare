using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Doctor
{
    public class CreateVisitRecordViewModel
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;

        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Start Time")]
        public TimeOnly StartTime { get; set; }

        [Display(Name = "End Time")]
        public TimeOnly EndTime { get; set; }

        [Display(Name = "Doctor Notes")]
        [Required(ErrorMessage = "Doctor notes are required.")]
        public string DoctorNotes { get; set; } = string.Empty;

        [Display(Name = "Diagnosis")]
        [Required(ErrorMessage = "Diagnosis is required.")]
        public string Diagnosis { get; set; } = string.Empty;

        [Display(Name = "Treatment")]
        public string? Treatment { get; set; }

        public List<PrescriptionInputViewModel> Prescriptions { get; set; } = new();
    }
}