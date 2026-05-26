namespace WebAPI.DTOs
{
    public class AppointmentStatsDTO
    {
        public int TotalAppointments { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int Missed { get; set; }
        public int InProgress { get; set; }
        public int CheckedIn { get; set; }
        public int Confirmed { get; set; }
        public int Requested { get; set; }
        public double CompletionRate { get; set; }
        public double CancellationRate { get; set; }
        public double MissedRate { get; set; }
    }

    public class AppointmentsByPeriodDTO
    {
        public string Period { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int Missed { get; set; }
    }

    public class DoctorWorkloadDTO
    {
        public string DoctorName { get; set; } = string.Empty;
        public List<string> Specializations { get; set; } = new();
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int MissedAppointments { get; set; }
        public double WorkloadRate { get; set; }
    }

    public class SpecializationStatsDTO
    {
        public string SpecializationName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
    }

    public class BusiestHourDTO
    {
        public int Hour { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public double AppointmentRate { get; set; }
    }

    public class CancellationReasonDTO
    {
        public string Reason { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Rate { get; set; }
    }

    public class MissedAppointmentRiskDTO
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string CPRNumber { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int MissedAppointments { get; set; }
        public double MissedRate { get; set; }
        public DateTime? LastMissedDate { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class DoctorLeaveImpactDTO
    {
        public string DoctorName { get; set; } = string.Empty;
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int AffectedAppointments { get; set; }
    }

    public class PrescriptionVolumeDTO
    {
        public string DoctorName { get; set; } = string.Empty;
        public string Specializations { get; set; } = string.Empty;
        public int VisitRecords { get; set; }
        public int PrescriptionCount { get; set; }
        public double PrescriptionRate { get; set; }
    }
}