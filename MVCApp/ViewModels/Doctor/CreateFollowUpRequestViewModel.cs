using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Doctor
{
    public class CreateFollowUpRequestViewModel
    {
        public int OriginalAppointmentId { get; set; }

        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;

        public int DoctorId { get; set; }
        public string DoctorFullName { get; set; } = string.Empty;

        public DateTime OriginalAppointmentDate { get; set; }

        [Required(ErrorMessage = "Recommended date is required.")]
        [DataType(DataType.Date)]
        public DateTime RecommendedDate { get; set; } = DateTime.Today.AddDays(14);

        [Required(ErrorMessage = "Start time is required.")]
        public TimeOnly StartTime { get; set; } = new TimeOnly(9, 0);

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason cannot be more than 500 characters.")]
        public string Reason { get; set; } = string.Empty;

        public string OriginalAppointmentDateDisplay => OriginalAppointmentDate.ToString("dd MMM yyyy");
    }
}