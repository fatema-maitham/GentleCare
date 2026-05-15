namespace MVCApp.ViewModels.ClinicManager
{
    // One leave row displayed in the Clinic Manager doctor leaves page.
    public class DoctorLeaveItemViewModel
    {
        public int DoctorLeaveId { get; set; }

        public int DoctorId { get; set; }

        public DateTime LeaveStartDate { get; set; }

        public DateTime LeaveEndDate { get; set; }

        public string LeaveReason { get; set; } = string.Empty;

        // View helper text
        public string LeaveDateText =>
            LeaveStartDate.Date == LeaveEndDate.Date
                ? LeaveStartDate.ToString("dd MMM yyyy")
                : $"{LeaveStartDate:dd MMM yyyy} - {LeaveEndDate:dd MMM yyyy}";

        public bool IsCurrentLeave =>
            LeaveStartDate.Date <= DateTime.Today &&
            LeaveEndDate.Date >= DateTime.Today;

        public bool IsUpcomingLeave =>
            LeaveStartDate.Date > DateTime.Today;

        public bool IsPastLeave =>
            LeaveEndDate.Date < DateTime.Today;

        public string LeaveStatusText
        {
            get
            {
                if (IsCurrentLeave)
                {
                    return "Current";
                }

                if (IsUpcomingLeave)
                {
                    return "Upcoming";
                }

                return "Past";
            }
        }
    }
}