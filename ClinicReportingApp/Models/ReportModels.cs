using System.ComponentModel.DataAnnotations;

namespace ClinicReportingApp.Models
{
    // ── Auth ──────────────────────────────────────────────────────────────────
    public class LoginRequest
    {
        [Required] public string Email    { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string   Token    { get; set; } = string.Empty;
        public string   FullName { get; set; } = string.Empty;
        public string   Email    { get; set; } = string.Empty;
        public string   Role     { get; set; } = string.Empty;
        public DateTime Expiry   { get; set; }
    }

    // GET /api/report/appointment-stats?from=&to=
    public class AppointmentStatsDto
    {
        public int    TotalAppointments { get; set; }
        public int    Completed         { get; set; }
        public int    Cancelled         { get; set; }
        public int    Missed            { get; set; }
        public int    Pending           { get; set; } // Requested + Confirmed + CheckedIn
        public double CancellationRate  { get; set; }
        public double MissedRate        { get; set; }
        public double CompletionRate { get; set; }
        public int InProgress { get; set; }


    }

    // GET /api/report/appointments-by-period
        public class AppointmentsByPeriodDto
    {
        public string Period { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int Missed { get; set; }
    }

    // GET /api/report/doctor-workload?from=&to=
    public class DoctorloadDto
    {
        public string       DoctorName            { get; set; } = string.Empty;
        public List<string> Specializations       { get; set; } = new();
        public int          TotalAppointments     { get; set; }
        public int          CompletedAppointments { get; set; }
        public int          CancelledAppointments { get; set; }
        public int          MissedAppointments    { get; set; }
        public double       WorkloadRate          { get; set; }
        public int           InProgress            { get; set; }
        
    }

    // GET /api/report/specialization-stats?from=&to=
    public class SpecializationStatsDto
    {
        public string SpecializationName    { get; set; } = string.Empty;
        public int    TotalAppointments     { get; set; }
        public int    CompletedAppointments { get; set; }
    }

    // GET /api/report/daily-summary
    public class DailySummaryDto
    {
        public string Date       { get; set; } = string.Empty;
        public int    Total      { get; set; }
        public int    Completed  { get; set; }
        public int    InProgress { get; set; }
        public int    Waiting    { get; set; }
        public int    Cancelled  { get; set; }
        public int    Upcoming   { get; set; }
    }

    // GET /api/report/patient-stats
    public class PatientStatsDto
    {
        public int    TotalPatients             { get; set; }
        public int    ActivePatients            { get; set; }
        public int    NewPatients               { get; set; }
        public double AvgAppointmentsPerPatient { get; set; }
    }

    // GET /api/report/prescription-stats
    public class PrescriptionStatsDto
    {
        public int                   TotalPrescriptions       { get; set; }
        public string                MostPrescribedMedication { get; set; } = string.Empty;
        public List<MedicationCount> TopMedications           { get; set; } = new();
    }

    public class MedicationCount
    {
        public string MedicationName { get; set; } = string.Empty;
        public int    Count          { get; set; }
    }

    public class DateRangeFilter
    {
        public DateTime? From { get; set; }
        public DateTime? To   { get; set; }
        public string FromStr => From?.ToString("yyyy-MM-dd") ?? "";
        public string ToStr   => To?.ToString("yyyy-MM-dd")   ?? "";
    }
}
