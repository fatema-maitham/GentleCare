namespace MVCApp.ViewModels.Appointment
{
    public class DoctorAppointmentDetailsViewModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public int? VisitRecordId { get; set; }
        public string? DoctorNotes { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public List<PrescriptionItemViewModel> Prescriptions { get; set; } = new();
        public List<string> AllowedNextStatuses { get; set; } = new();
    }

    public class PrescriptionItemViewModel
    {
        public int Id { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
    }
}
