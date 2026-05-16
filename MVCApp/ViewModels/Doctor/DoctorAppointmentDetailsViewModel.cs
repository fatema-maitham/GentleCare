using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Doctor Appointment Details page.
    // It shows appointment information, patient details, visit record, prescriptions,
    // and the next allowed appointment status actions.
    public class DoctorAppointmentDetailsViewModel
    {
        // Appointment details.
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Current appointment workflow status.
        public string StatusName { get; set; } = string.Empty;

        // Appointment notes and cancellation reason, if available.
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }

        // Patient details shown to the doctor.
        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string PatientReferenceNumber { get; set; } = string.Empty;
        public string PatientCprNumber { get; set; } = string.Empty;

        // Visit record details, if the appointment already has a visit record.
        public string? DoctorNotes { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }

        // Flags used by the view to show or hide action buttons.
        public bool HasVisitRecord { get; set; }
        public bool CanCreateVisitRecord { get; set; }
        public bool CanEditVisitRecord { get; set; }
        public bool CanUpdateStatus { get; set; }
        public bool CanCreateFollowUpRequest { get; set; }

        // Dropdown options for valid next appointment statuses.
        public List<SelectListItem> AvailableNextStatuses { get; set; } = new();

        // Prescriptions linked to the visit record.
        public List<PrescriptionInputViewModel> Prescriptions { get; set; } = new();

        // Display helpers for the view.
        public string AppointmentDateDisplay => AppointmentDate.ToString("dd MMM yyyy");
        public string TimeDisplay => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";
        public bool HasPrescriptions => Prescriptions.Any();
        public bool HasCancellationReason => !string.IsNullOrWhiteSpace(CancellationReason);
    }
}