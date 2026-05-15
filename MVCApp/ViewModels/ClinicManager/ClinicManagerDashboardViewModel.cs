namespace MVCApp.ViewModels.ClinicManager
{
    // Dashboard data shown to the Clinic Manager.
    public class ClinicManagerDashboardViewModel
    {
        // Main clinic numbers
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int TotalPatients { get; set; }

        // Appointment summary
        public int AppointmentsToday { get; set; }
        public int RequestedAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CheckedInAppointments { get; set; }
        public int InProgressAppointments { get; set; }
        public int CompletedAppointmentsToday { get; set; }
        public int CancelledAppointmentsToday { get; set; }
        public int MissedAppointmentsToday { get; set; }

        // Availability summary
        public int DoctorsOnLeaveToday { get; set; }
        public int ImpactedAppointmentsToday { get; set; }

        // Quick percentages
        public double CompletionRateToday { get; set; }
        public double CancellationRateToday { get; set; }
        public double MissedRateToday { get; set; }

        // Dashboard lists
        public List<ClinicManagerDashboardAppointmentViewModel> TodayAppointments { get; set; } = new();

        public List<ClinicManagerDashboardDoctorWorkloadViewModel> DoctorWorkloads { get; set; } = new();

        public List<ClinicManagerDashboardLeaveViewModel> DoctorsOnLeave { get; set; } = new();

        // View helpers
        public bool HasAppointmentsToday => TodayAppointments.Any();

        public bool HasDoctorWorkloads => DoctorWorkloads.Any();

        public bool HasDoctorsOnLeave => DoctorsOnLeave.Any();
    }

    // Small appointment item shown on the dashboard.
    public class ClinicManagerDashboardAppointmentViewModel
    {
        public int AppointmentId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public string TimeText => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";
    }

    // Doctor workload summary shown on the dashboard.
    public class ClinicManagerDashboardDoctorWorkloadViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public int TotalAppointmentsToday { get; set; }

        public int CompletedAppointmentsToday { get; set; }

        public int RemainingAppointmentsToday { get; set; }
    }

    // Doctor leave summary shown on the dashboard.
    public class ClinicManagerDashboardLeaveViewModel
    {
        public int DoctorLeaveId { get; set; }

        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string LeaveReason { get; set; } = string.Empty;

        public DateTime LeaveStartDate { get; set; }

        public DateTime LeaveEndDate { get; set; }

        public string LeaveDateText =>
            LeaveStartDate.Date == LeaveEndDate.Date
                ? LeaveStartDate.ToString("dd MMM yyyy")
                : $"{LeaveStartDate:dd MMM yyyy} - {LeaveEndDate:dd MMM yyyy}";
    }
}