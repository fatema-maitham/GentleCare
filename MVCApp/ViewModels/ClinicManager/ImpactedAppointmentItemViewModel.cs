namespace MVCApp.ViewModels.ClinicManager
{
    // One appointment affected by a doctor schedule or leave change.
    public class ImpactedAppointmentItemViewModel
    {
        public int AppointmentId { get; set; }

        public int DoctorId { get; set; }

        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string StatusName { get; set; } = string.Empty;

        // Example: Leave / Schedule
        public string ImpactType { get; set; } = string.Empty;

        // Example: Doctor is on leave on this date.
        public string ImpactReason { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        // View helper text
        public string TimeText => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";

        public string DateText => AppointmentDate.ToString("dd MMM yyyy");

        public string AppointmentText =>
            $"{DateText}, {TimeText}";

        public bool CanCancel =>
            StatusName != "Completed" &&
            StatusName != "Cancelled" &&
            StatusName != "Missed";

        public bool CanNotify =>
            StatusName != "Completed" &&
            StatusName != "Cancelled";
    }
}