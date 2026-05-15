namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to view and manage one doctor's leave periods.
    public class ManageDoctorLeavesViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string DoctorEmail { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        // Leave periods for this doctor.
        public List<DoctorLeaveItemViewModel> Leaves { get; set; } = new();

        // View helper properties
        public bool HasLeaves => Leaves.Any();

        public int TotalLeaves => Leaves.Count;

        public int UpcomingLeaves =>
            Leaves.Count(l => l.LeaveStartDate.Date >= DateTime.Today);

        public int CurrentLeaves =>
            Leaves.Count(l =>
                l.LeaveStartDate.Date <= DateTime.Today &&
                l.LeaveEndDate.Date >= DateTime.Today);

        public string EmptyMessage => "No leave periods have been added for this doctor yet.";
    }
}