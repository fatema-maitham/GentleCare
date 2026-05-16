namespace MVCApp.ViewModels.Doctor
{
    // Represents one appointment row/card in the doctor's appointment list.
    public class DoctorAppointmentListItemViewModel
    {
        // Appointment primary key used for details and actions.
        public int AppointmentId { get; set; }

        // Patient details shown in the appointment list.
        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string PatientReferenceNumber { get; set; } = string.Empty;

        // Appointment date and time slot.
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Current appointment workflow status.
        public string StatusName { get; set; } = string.Empty;

        // Optional appointment notes entered during booking or workflow.
        public string? Notes { get; set; }

        // Display helpers for the view.
        public string AppointmentDateDisplay => AppointmentDate.ToString("dd MMM yyyy");
        public string TimeDisplay => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";

        // Used by the view to show action buttons safely.
        public bool CanViewDetails => AppointmentId > 0;
        public bool CanUpdateStatus =>
            StatusName == "Confirmed" ||
            StatusName == "Checked In" ||
            StatusName == "In Progress";
    }
}