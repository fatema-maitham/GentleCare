namespace MVCApp.ViewModels.ClinicManager
{
    // Represents one appointment that may be affected by a doctor schedule change,
    // doctor leave, or unavailable time slot.
    public class ImpactedAppointmentItemViewModel
    {
        // Unique appointment ID.
        public int AppointmentId { get; set; }

        // Doctor assigned to this appointment.
        public int DoctorId { get; set; }

        // Patient assigned to this appointment.
        public int PatientId { get; set; }

        // Patient full name shown in the impacted appointments list.
        public string PatientName { get; set; } = string.Empty;

        // Doctor full name shown in the impacted appointments list.
        public string DoctorName { get; set; } = string.Empty;

        // Appointment date that became affected.
        public DateTime AppointmentDate { get; set; }

        // Original appointment start time.
        public TimeOnly StartTime { get; set; }

        // Original appointment end time.
        public TimeOnly EndTime { get; set; }

        // Current appointment status, such as Confirmed, Completed, Cancelled, or Missed.
        public string StatusName { get; set; } = string.Empty;

        // Short label describing the type of impact.
        // Example: "Doctor Leave", "Schedule Change", "Unavailable Slot".
        public string ImpactType { get; set; } = string.Empty;

        // Clear explanation of why this appointment is affected.
        public string ImpactReason { get; set; } = string.Empty;

        // Optional appointment notes shown to the clinic manager.
        public string Notes { get; set; } = string.Empty;

        // Smart alternative slots suggested for rescheduling this impacted appointment.
        public List<AppointmentRescheduleSuggestionViewModel> RescheduleSuggestions { get; set; } = new();

        // Formats the appointment time range for display.
        public string TimeText => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";

        // Formats the appointment date for display.
        public string DateText => AppointmentDate.ToString("dd MMM yyyy");

        // Combines the formatted date and time into one readable appointment text.
        public string AppointmentText => $"{DateText}, {TimeText}";

        // Allows cancellation only if the appointment is still active.
        // Completed, Cancelled, and Missed appointments should not be cancelled again.
        public bool CanCancel =>
            StatusName != "Completed" &&
            StatusName != "Cancelled" &&
            StatusName != "Missed";

        // Returns true when at least one suggested reschedule slot is available.
        public bool HasSuggestions => RescheduleSuggestions.Any();
    }
}