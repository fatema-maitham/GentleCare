using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to create or edit a doctor's schedule.
    public class DoctorScheduleFormViewModel
    {
        // Null when creating, has value when editing.
        public int? DoctorScheduleId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Day of week is required.")]
        [Display(Name = "Day of Week")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [Display(Name = "Start Time")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [Display(Name = "End Time")]
        public TimeOnly EndTime { get; set; }

        [Required(ErrorMessage = "Slot duration is required.")]
        [Range(5, 240, ErrorMessage = "Slot duration must be between 5 and 240 minutes.")]
        [Display(Name = "Slot Duration Minutes")]
        public int SlotDurationMinutes { get; set; } = 30;

        // Helper property for the view.
        public bool IsEditMode => DoctorScheduleId.HasValue;

        public string PageTitle => IsEditMode ? "Edit Doctor Schedule" : "Create Doctor Schedule";
    }
}