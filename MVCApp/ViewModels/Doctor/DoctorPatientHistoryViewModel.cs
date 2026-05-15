namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Doctor Patient History page.
    // It shows patient information and previous visit records for patients linked to this doctor.
    public class DoctorPatientHistoryViewModel
    {
        // Patient basic details.
        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string CPRNumber { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;

        // Optional patient profile details.
        public DateTime? DateOfBirth { get; set; }
        public string? BloodType { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }

        // Previous visits for this patient with the logged-in doctor.
        public List<DoctorPatientVisitItemViewModel> Visits { get; set; } = new();

        // Display helpers for the view.
        public bool HasVisits => Visits.Any();
        public int TotalVisits => Visits.Count;

        public string DateOfBirthDisplay =>
            DateOfBirth.HasValue ? DateOfBirth.Value.ToString("dd MMM yyyy") : "Not recorded";

        public string BloodTypeDisplay =>
            string.IsNullOrWhiteSpace(BloodType) ? "Not recorded" : BloodType;
    }

    // Represents one previous visit row/card in the patient's history page.
    public class DoctorPatientVisitItemViewModel
    {
        // Appointment linked to this visit.
        public int AppointmentId { get; set; }

        // Visit appointment date and time.
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Appointment status during/after the visit.
        public string StatusName { get; set; } = string.Empty;

        // Medical record details written by the doctor.
        public string? DoctorNotes { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }

        // Number of prescriptions linked to this visit.
        public int PrescriptionCount { get; set; }

        // Display helpers for the view.
        public string AppointmentDateDisplay => AppointmentDate.ToString("dd MMM yyyy");
        public string TimeDisplay => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";
        public bool HasPrescriptions => PrescriptionCount > 0;
    }
}