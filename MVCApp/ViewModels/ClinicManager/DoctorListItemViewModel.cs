namespace MVCApp.ViewModels.ClinicManager
{
    // One doctor item shown in the Clinic Manager doctors list.
    public class DoctorListItemViewModel
    {
        public int DoctorId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string Bio { get; set; } = string.Empty;

        public List<string> Specializations { get; set; } = new();

        public int TodayAppointmentsCount { get; set; }

        public int UpcomingAppointmentsCount { get; set; }

        public int CompletedAppointmentsCount { get; set; }

        public bool IsOnLeaveToday { get; set; }

        // View helper text
        public string StatusText => IsActive ? "Active" : "Inactive";

        public string LeaveStatusText => IsOnLeaveToday ? "On Leave Today" : "Available";

        public string SpecializationsText =>
            Specializations.Any()
                ? string.Join(", ", Specializations)
                : "No specializations assigned";
    }
}