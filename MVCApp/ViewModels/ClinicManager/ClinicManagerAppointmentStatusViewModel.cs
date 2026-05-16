using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to update an appointment status.
    public class ClinicManagerAppointmentStatusViewModel
    {
        [Required]
        public int AppointmentId { get; set; }

        public int DoctorId { get; set; }

        public int PatientId { get; set; }

        // Display information
        public string PatientName { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string CurrentStatusName { get; set; } = string.Empty;

        // New selected status
        [Required(ErrorMessage = "Please select a new status.")]
        [Display(Name = "New Status")]
        public string NewStatusName { get; set; } = string.Empty;

        // Required only if cancelling the appointment
        [StringLength(300, ErrorMessage = "Cancellation reason cannot be longer than 300 characters.")]
        [Display(Name = "Cancellation Reason")]
        public string? CancellationReason { get; set; }

        // Dropdown for valid next statuses only
        public List<SelectListItem> StatusOptions { get; set; } = new();

        // Helper text for the view
        public string DateText => AppointmentDate.ToString("dd MMM yyyy");

        public string TimeText => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";

        public string AppointmentText => $"{DateText}, {TimeText}";

        public bool IsCancellationSelected => NewStatusName == "Cancelled";

        public bool HasStatusOptions => StatusOptions.Any();
    }
}