namespace MVCApp.ViewModels.ClinicManager
{
    // One appointment row displayed in the Clinic Manager appointments page.
    public class ClinicManagerAppointmentItemViewModel
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public string PatientReferenceNumber { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string SpecializationName { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public string CancellationReason { get; set; } = string.Empty;

        // View helper text
        public string DateText => AppointmentDate.ToString("dd MMM yyyy");

        public string TimeText => $"{StartTime:HH\\:mm} - {EndTime:HH\\:mm}";

        public string AppointmentText => $"{DateText}, {TimeText}";

        public bool CanViewDetails => AppointmentId > 0;

        public bool CanUpdateStatus =>
            StatusName != "Completed" &&
            StatusName != "Cancelled" &&
            StatusName != "Missed";
    }
}