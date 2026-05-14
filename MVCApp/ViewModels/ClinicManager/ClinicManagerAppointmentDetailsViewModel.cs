namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to view full appointment details.
    public class ClinicManagerAppointmentDetailsViewModel
    {
        public int AppointmentId { get; set; }

        // Patient information
        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string PatientEmail { get; set; } = string.Empty;

        public string PatientPhoneNumber { get; set; } = string.Empty;

        public string PatientReferenceNumber { get; set; } = string.Empty;

        public string PatientCprNumber { get; set; } = string.Empty;

        // Doctor information
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string DoctorEmail { get; set; } = string.Empty;

        public string DoctorLicenseNumber { get; set; } = string.Empty;

        public string SpecializationName { get; set; } = string.Empty;

        // Appointment information
        public DateTime AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public string CancellationReason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Visit record summary
        public bool HasVisitRecord { get; set; }

        public int? VisitRecordId { get; set; }

        public string Diagnosis { get; set; } = string.Empty;

        public string Treatment { get; set; } = string.Empty;

        public string DoctorNotes { get; set; } = string.Empty;

        // Prescription summary
        public int PrescriptionCount { get; set; }

        // View helper text
        public string DateText => AppointmentDate.ToString("dd MMM yyyy");

        public string TimeText => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";

        public string AppointmentText => $"{DateText}, {TimeText}";

        public bool CanUpdateStatus =>
            StatusName != "Completed" &&
            StatusName != "Cancelled" &&
            StatusName != "Missed";

        public bool IsCancelled => StatusName == "Cancelled";

        public bool IsCompleted => StatusName == "Completed";
    }
}