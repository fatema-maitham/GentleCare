using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistUpdateAppointmentStatusViewModel
    {
        public int AppointmentId { get; set; }

        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a new status.")]
        [Display(Name = "New Status")]
        public string NewStatus { get; set; } = string.Empty;

        [Display(Name = "Cancellation Reason")]
        public string? CancellationReason { get; set; }

        public List<SelectListItem> AllowedStatuses { get; set; } = new();
    }
}