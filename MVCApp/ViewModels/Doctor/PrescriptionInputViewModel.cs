using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used for one prescription row inside create/edit visit record forms.
    // A visit record can have zero, one, or multiple prescriptions.
    public class PrescriptionInputViewModel
    {
        // Medicine name prescribed by the doctor.
        [StringLength(150, ErrorMessage = "Medication name cannot exceed 150 characters.")]
        [Display(Name = "Medication Name")]
        public string MedicationName { get; set; } = string.Empty;

        // Example: 500mg, 1 tablet, 5ml.
        [StringLength(100, ErrorMessage = "Dosage cannot exceed 100 characters.")]
        public string Dosage { get; set; } = string.Empty;

        // Example: Twice daily, once before sleep, every 8 hours.
        [StringLength(150, ErrorMessage = "Frequency cannot exceed 150 characters.")]
        public string Frequency { get; set; } = string.Empty;

        // Number of days the patient should use the medication.
        [Range(0, 365, ErrorMessage = "Duration must be between 0 and 365 days.")]
        [Display(Name = "Duration Days")]
        public int DurationDays { get; set; }

        // Extra instructions such as before food or after food.
        [StringLength(500, ErrorMessage = "Instructions cannot exceed 500 characters.")]
        public string? Instructions { get; set; }

        // Display helpers for the view.
        public bool HasMedication =>
            !string.IsNullOrWhiteSpace(MedicationName) ||
            !string.IsNullOrWhiteSpace(Dosage) ||
            !string.IsNullOrWhiteSpace(Frequency) ||
            DurationDays > 0 ||
            !string.IsNullOrWhiteSpace(Instructions);

        public string DurationDisplay =>
            DurationDays > 0 ? $"{DurationDays} day(s)" : "Not specified";
    }
}