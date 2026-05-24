using System.ComponentModel.DataAnnotations;

namespace ClinicReportingApp.Models
{
    // Auth models
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
    ;
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }

    // Appointment summary report
    public class AppointmentStatsDto
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

    // Appointments grouped by month or period
    public class AppointmentsByPeriodDto
    {
        public string Period { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int Missed { get; set; }
    }

    // Doctor workload report
    public class DoctorloadDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public List<string> Specializations { get; set; } = new();

        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int MissedAppointments { get; set; }
        public int InProgress { get; set; }

        public double WorkloadRate { get; set; }
    }

    // Specialization report
    public class SpecializationStatsDto
    {
        public string SpecializationName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
    }

    // Daily appointment status summary
    public class DailySummaryDto
    {
        public string Date { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int Waiting { get; set; }
        public int Cancelled { get; set; }
        public int Missed { get; set; }
        public int Upcoming { get; set; }
    }

    // Patient activity report
    public class PatientStatsDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;

        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int MissedAppointments { get; set; }

        public bool IsNewPatient { get; set; }
    }

    // Prescription summary report
    public class PrescriptionStatsDto
    {
        public int TotalPrescriptions { get; set; }
        public string MostPrescribedMedication { get; set; } = string.Empty;
        public List<MedicationCount> TopMedications { get; set; } = new();
    }

    public class MedicationCount
    {
        public string MedicationName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // Busiest appointment hours report
    public class BusiestHourDto
    {
        public int Hour { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public double AppointmentRate { get; set; }
    }

    // Cancellation reason analysis
    public class CancellationReasonDto
    {
        public string Reason { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Rate { get; set; }
    }

    // Patients with missed appointment risk
    public class MissedAppointmentRiskDto
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

    // Doctor leave impact report
    public class DoctorLeaveImpactDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int AffectedAppointments { get; set; }
    }

    // Prescription volume by doctor
    public class PrescriptionVolumeDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public string Specializations { get; set; } = string.Empty;
        public int VisitRecords { get; set; }
        public int PrescriptionCount { get; set; }
        public double PrescriptionRate { get; set; }
    }

    // Shared date filter
    public class DateRangeFilter
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string FromStr => From?.ToString("yyyy-MM-dd") ?? "";
        public string ToStr => To?.ToString("yyyy-MM-dd") ?? "";
    }
}