using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Appointment
{
    public class UpdateAppointmentStatusViewModel
    {
        public int AppointmentId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public string? CancellationReason { get; set; }
    }
}
