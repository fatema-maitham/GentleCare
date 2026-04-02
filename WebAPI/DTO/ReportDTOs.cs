namespace WebAPI.DTOs
{
    public class AppointmentStatsDTO
    {
        public int TotalAppointments { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int Missed { get; set; }
        public int Pending { get; set; }
        public double CancellationRate { get; set; }
        public double MissedRate { get; set; }
    }

    public class DoctorWorkloadDTO
    {
        public string DoctorName { get; set; } = string.Empty;
        public List<string> Specializations { get; set; } = new();
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int MissedAppointments { get; set; }
    }

    public class SpecializationStatsDTO
    {
        public string SpecializationName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
    }
}