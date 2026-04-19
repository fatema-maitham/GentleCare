using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Appointment
{
    public class CreatePrescriptionViewModel
    {
        public int VisitRecordId { get; set; }
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Medication Name")]
        public string MedicationName { get; set; } = string.Empty;

        [Required]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        public string Frequency { get; set; } = string.Empty;

        [Range(1, 365)]
        [Display(Name = "Duration (Days)")]
        public int DurationDays { get; set; }

        public string? Instructions { get; set; }
    }
}
