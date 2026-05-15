namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used to display prescriptions written by the logged-in doctor.
    // Each prescription is linked to a visit record and appointment.
    public class DoctorPrescriptionViewModel
    {
        // Prescription primary key.
        public int PrescriptionId { get; set; }

        // Related visit record and appointment.
        public int VisitRecordId { get; set; }
        public int AppointmentId { get; set; }

        // Patient details shown with the prescription.
        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string PatientReferenceNumber { get; set; } = string.Empty;

        // Appointment date connected to this prescription.
        public DateTime AppointmentDate { get; set; }

        // Prescription details.
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }

        // Date the prescription was created.
        public DateTime CreatedAt { get; set; }

        // Display helpers for the view.
        public string AppointmentDateDisplay => AppointmentDate.ToString("dd MMM yyyy");
        public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy HH:mm");
        public string DurationDisplay => $"{DurationDays} day(s)";
        public bool HasInstructions => !string.IsNullOrWhiteSpace(Instructions);
    }
}