namespace WebAPI.DTOs
{
    public class AppointmentLookupRequestDTO
    {
        public string CPRNumber { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
    }

    public class PublicLookupResponseDTO
    {
        public List<AppointmentLookupResponseDTO> UpcomingAppointments { get; set; } = new();
        public List<PublicVisitSummaryDTO> RecentVisits { get; set; } = new();
    }

    public class AppointmentLookupResponseDTO
    {
        public int AppointmentId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string SpecializationName { get; set; } = "General Clinic";

        public string? Notes { get; set; }
    }

    public class PublicVisitSummaryDTO
    {
        public DateTime VisitDate { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string? Diagnosis { get; set; }

        public string? Treatment { get; set; }

        public string? DoctorNotes { get; set; }
    }

    public class UpdateAppointmentStatusDTO
    {
        public string Status { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
    }

    public class CreateAppointmentDTO
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
    }

    public class AppointmentResponseDTO
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}