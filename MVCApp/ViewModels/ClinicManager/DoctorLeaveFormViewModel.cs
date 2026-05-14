using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to create or edit a doctor's leave period.
    public class DoctorLeaveFormViewModel
    {
        // Null when creating, has value when editing.
        public int? DoctorLeaveId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Leave start date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Leave Start Date")]
        public DateTime LeaveStartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Leave end date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Leave End Date")]
        public DateTime LeaveEndDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Leave reason is required.")]
        [StringLength(300, ErrorMessage = "Leave reason cannot be longer than 300 characters.")]
        [Display(Name = "Leave Reason")]
        public string LeaveReason { get; set; } = string.Empty;

        // Helper properties for the view.
        public bool IsEditMode => DoctorLeaveId.HasValue;

        public string PageTitle => IsEditMode ? "Edit Doctor Leave" : "Create Doctor Leave";
    }
}