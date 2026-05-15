using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Create Visit Record page.
    // The doctor uses this form to record notes, diagnosis, treatment, and prescriptions.
    public class CreateVisitRecordViewModel
    {
        // Appointment linked to this visit record.
        public int AppointmentId { get; set; }

        // Patient details shown on the form for confirmation.
        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;

        // Appointment date and time shown on the form.
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Doctor notes are required because they explain what happened during the visit.
        [Required(ErrorMessage = "Doctor notes are required.")]
        [StringLength(1000, ErrorMessage = "Doctor notes cannot exceed 1000 characters.")]
        [Display(Name = "Doctor Notes")]
        public string DoctorNotes { get; set; } = string.Empty;

        // Diagnosis is required for the completed medical visit.
        [Required(ErrorMessage = "Diagnosis is required.")]
        [StringLength(500, ErrorMessage = "Diagnosis cannot exceed 500 characters.")]
        public string Diagnosis { get; set; } = string.Empty;

        // Treatment is optional because some visits may only need advice or observation.
        [StringLength(1000, ErrorMessage = "Treatment cannot exceed 1000 characters.")]
        public string? Treatment { get; set; }

        // Optional prescriptions linked to this visit record.
        public List<PrescriptionInputViewModel> Prescriptions { get; set; } = new();

        // Display helpers for the view.
        public string AppointmentDateDisplay => AppointmentDate.ToString("dd MMM yyyy");
        public string TimeDisplay => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";
        public bool HasPrescriptions => Prescriptions.Any();
    }
}