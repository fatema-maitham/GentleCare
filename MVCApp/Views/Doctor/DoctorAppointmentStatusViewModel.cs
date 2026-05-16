using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Doctor
{
    public class DoctorAppointmentStatusViewModel
    {
        public int AppointmentId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string CurrentStatusName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select the new status.")]
        public string NewStatusName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? StatusNote { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}