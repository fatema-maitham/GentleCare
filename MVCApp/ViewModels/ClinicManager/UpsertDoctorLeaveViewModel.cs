using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.ClinicManager
{
    public class UpsertDoctorLeaveViewModel
    {
        public int? DoctorLeaveId { get; set; }
        public int DoctorId { get; set; }

        [Display(Name = "Leave Start Date")]
        public DateTime LeaveStartDate { get; set; }

        [Display(Name = "Leave End Date")]
        public DateTime LeaveEndDate { get; set; }

        [Required]
        [Display(Name = "Leave Reason")]
        public string LeaveReason { get; set; } = string.Empty;
    }
}
