using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Doctor
{
    public class PrescriptionInputViewModel
    {
        [Display(Name = "Medication Name")]
        public string MedicationName { get; set; } = string.Empty;

        [Display(Name = "Dosage")]
        public string Dosage { get; set; } = string.Empty;

        [Display(Name = "Frequency")]
        public string Frequency { get; set; } = string.Empty;

        [Display(Name = "Duration (Days)")]
        public int DurationDays { get; set; }

        [Display(Name = "Instructions")]
        public string? Instructions { get; set; }
    }
}