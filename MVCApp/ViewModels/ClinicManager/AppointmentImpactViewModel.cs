namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to review appointments affected
    // by doctor schedule or leave changes.
    public class AppointmentImpactViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string DoctorEmail { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        // Date range used to check impacted appointments.
        public DateTime FromDate { get; set; } = DateTime.Today;

        public DateTime ToDate { get; set; } = DateTime.Today.AddMonths(1);

        // Appointments affected by schedule or leave changes.
        public List<ImpactedAppointmentItemViewModel> ImpactedAppointments { get; set; } = new();

        // View helper properties
        public bool HasImpactedAppointments => ImpactedAppointments.Any();

        public int TotalImpactedAppointments => ImpactedAppointments.Count;

        public int LeaveImpacts =>
            ImpactedAppointments.Count(a => a.ImpactType == "Leave");

        public int ScheduleImpacts =>
            ImpactedAppointments.Count(a => a.ImpactType == "Schedule");

        public string DateRangeText =>
            FromDate.Date == ToDate.Date
                ? FromDate.ToString("dd MMM yyyy")
                : $"{FromDate:dd MMM yyyy} - {ToDate:dd MMM yyyy}";

        public string EmptyMessage =>
            "No impacted appointments were found for this doctor in the selected date range.";
    }
}