namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel for the Clinic Manager doctor details page.
    // It shows doctor profile, specializations, schedules, leaves, and appointment summary.
    public class DoctorDetailsViewModel
    {
        public int DoctorId { get; set; }

        public string UserId { get; set; } = string.Empty;

        // Doctor profile
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Specializations
        public List<string> Specializations { get; set; } = new();

        // Doctor schedule and leave summary
        public List<DoctorScheduleItemViewModel> Schedules { get; set; } = new();

        public List<DoctorLeaveItemViewModel> Leaves { get; set; } = new();

        // Appointment summary
        public int TotalAppointments { get; set; }

        public int TodayAppointments { get; set; }

        public int UpcomingAppointments { get; set; }

        public int RequestedAppointments { get; set; }

        public int ConfirmedAppointments { get; set; }

        public int CompletedAppointments { get; set; }

        public int CancelledAppointments { get; set; }

        public int MissedAppointments { get; set; }

        public bool IsOnLeaveToday { get; set; }

        // View helper properties
        public string StatusText => IsActive ? "Active" : "Inactive";

        public string LeaveStatusText => IsOnLeaveToday ? "On Leave Today" : "Available Today";

        public string SpecializationsText =>
            Specializations.Any()
                ? string.Join(", ", Specializations)
                : "No specializations assigned";

        public bool HasSpecializations => Specializations.Any();

        public bool HasSchedules => Schedules.Any();

        public bool HasLeaves => Leaves.Any();

        public bool HasAppointments => TotalAppointments > 0;
    }
}