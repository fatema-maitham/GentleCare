namespace MVCApp.ViewModels.ClinicManager
{
    public class DoctorLeaveItemViewModel
    {
        public int DoctorLeaveId { get; set; }

        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }

        public string LeaveReason { get; set; } = string.Empty;
    }
}
