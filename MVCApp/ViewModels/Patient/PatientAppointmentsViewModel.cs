namespace MVCApp.ViewModels.Patient
{
    public class PatientAppointmentsViewModel
    {
        public int AppointmentId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public bool CanCancel =>
            Status == "Requested" || Status == "Confirmed";
    }
}