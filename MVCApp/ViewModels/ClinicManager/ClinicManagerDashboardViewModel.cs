namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel for the Clinic Manager dashboard page.
    // It contains summary numbers and quick clinic overview data.
    public class ClinicManagerDashboardViewModel
    {
        // Main statistics cards
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int TotalPatients { get; set; }

        public int AppointmentsToday { get; set; }
        public int RequestedAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointmentsToday { get; set; }
        public int CancelledAppointmentsToday { get; set; }
        public int MissedAppointmentsToday { get; set; }

        public int DoctorsOnLeaveToday { get; set; }
        public int ImpactedAppointmentsToday { get; set; }

        // Quick report percentages
        public double CompletionRateToday { get; set; }
        public double CancellationRateToday { get; set; }
        public double MissedRateToday { get; set; }

        // Lists shown on dashboard
        public List<ClinicManagerDashboardAppointmentViewModel> TodayAppointments { get; set; } = new();
        public List<ClinicManagerDashboardDoctorWorkloadViewModel> DoctorWorkloads { get; set; } = new();
        public List<ClinicManagerDashboardLeaveViewModel> DoctorsOnLeave { get; set; } = new();

        // Helper message for empty dashboard sections
        public bool HasAppointmentsToday => TodayAppointments.Any();
        public bool HasDoctorsOnLeaveToday => DoctorsOnLeave.Any();
        public bool HasDoctorWorkload => DoctorWorkloads.Any();
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
        public string SpecializationName { get; set; } = string.Empty;
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
    }
}