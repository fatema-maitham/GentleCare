namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used to display the details of one visit record.
    // It shows the doctor's notes, diagnosis, treatment, and prescriptions.
    public class VisitRecordDetailsViewModel
    {
        // Visit record primary key.
        public int VisitRecordId { get; set; }

        // Appointment linked to this visit record.
        public int AppointmentId { get; set; }

        // Patient details shown with the visit record.
        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string PatientReferenceNumber { get; set; } = string.Empty;
        public string PatientCprNumber { get; set; } = string.Empty;

        // Appointment date and time for this visit.
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Medical information written by the doctor.
        public string DoctorNotes { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Treatment { get; set; }

        // Prescriptions linked to this visit record.
        public List<PrescriptionInputViewModel> Prescriptions { get; set; } = new();

        // Date the visit record was created.
        public DateTime CreatedAt { get; set; }

        // Display helpers for the view.
        public string AppointmentDateDisplay => AppointmentDate.ToString("dd MMM yyyy");
        public string TimeDisplay => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";
        public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy HH:mm");
        public bool HasTreatment => !string.IsNullOrWhiteSpace(Treatment);
        public bool HasPrescriptions => Prescriptions.Any();
    }
}