using System.Text.Json.Serialization;

namespace MVCApp.ViewModels.Public
{
    public class PublicAppointmentLookupViewModel
    {
        public string CPRNumber { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;

        public bool HasSearched { get; set; }
        public string? ErrorMessage { get; set; }

        public List<PublicAppointmentResultViewModel> UpcomingAppointments { get; set; } = new();
        public List<PublicVisitSummaryViewModel> RecentVisits { get; set; } = new();
    }

    public class PublicLookupResponseViewModel
    {
        public List<PublicAppointmentResultViewModel> UpcomingAppointments { get; set; } = new();
        public List<PublicVisitSummaryViewModel> RecentVisits { get; set; } = new();
    }

    public class PublicAppointmentResultViewModel
    {
        public int AppointmentId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string StatusName { get; set; } = string.Empty;

        public string SpecializationName { get; set; } = "General Clinic";

        public string? Notes { get; set; }
    }

    public class PublicVisitSummaryViewModel
    {
        public DateTime VisitDate { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string? Diagnosis { get; set; }

        public string? Treatment { get; set; }

        public string? DoctorNotes { get; set; }
    }
}