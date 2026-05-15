namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to view and manage one doctor's weekly schedule.
    public class ManageDoctorScheduleViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string DoctorEmail { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        // Weekly schedule rows for this doctor.
        public List<DoctorScheduleItemViewModel> Schedules { get; set; } = new();

        // View helper properties
        public bool HasSchedules => Schedules.Any();

        public int TotalSchedules => Schedules.Count;

        public string EmptyMessage => "No schedule has been added for this doctor yet.";
    }
}