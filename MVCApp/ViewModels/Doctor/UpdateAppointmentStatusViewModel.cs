using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Doctor
{
    public class UpdateAppointmentStatusViewModel
    {
        public int AppointmentId { get; set; }

        [Display(Name = "Current Status")]
        public string CurrentStatusName { get; set; } = string.Empty;

        [Display(Name = "New Status")]
        [Required(ErrorMessage = "Please select a new status.")]
        public string NewStatusName { get; set; } = string.Empty;

        public List<SelectListItem> AvailableStatuses { get; set; } = new();
    }
}