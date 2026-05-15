using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Update Appointment Status page.
    // It allows the doctor to move an appointment to the next valid workflow status.
    public class UpdateAppointmentStatusViewModel
    {
        // Appointment being updated.
        public int AppointmentId { get; set; }

        // Current status displayed to the doctor.
        public string CurrentStatusName { get; set; } = string.Empty;

        // New status selected by the doctor.
        [Required(ErrorMessage = "Please select the new appointment status.")]
        [Display(Name = "New Status")]
        public string NewStatusName { get; set; } = string.Empty;

        // Dropdown options containing only valid next statuses.
        public List<SelectListItem> AvailableStatuses { get; set; } = new();

        // Optional reason or note for the status update.
        [StringLength(500, ErrorMessage = "Status note cannot exceed 500 characters.")]
        [Display(Name = "Status Note")]
        public string? StatusNote { get; set; }

        // Display helpers for the view.
        public bool HasAvailableStatuses => AvailableStatuses.Any();
        public string CurrentStatusDisplay =>
            string.IsNullOrWhiteSpace(CurrentStatusName) ? "Not set" : CurrentStatusName;
    }
}