using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Patient
{
    public class PatientBookAppointmentViewModel
    {
        [Display(Name = "Specialization")]
        [Required(ErrorMessage = "Please select a specialization.")]
        public int? SpecializationId { get; set; }

        [Display(Name = "Doctor")]
        [Required(ErrorMessage = "Please select a doctor.")]
        public int? DoctorId { get; set; }

        [Display(Name = "Appointment Date")]
        [Required(ErrorMessage = "Please select an appointment date.")]
        [DataType(DataType.Date)]
        public DateTime? AppointmentDate { get; set; }

        [Display(Name = "Start Time")]
        [Required(ErrorMessage = "Please select a start time.")]
        public string? StartTime { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        public List<SelectListItem> SpecializationOptions { get; set; } = new();

        public List<SelectListItem> DoctorOptions { get; set; } = new();

        public List<SelectListItem> AvailableSlotOptions { get; set; } = new();
    }
}