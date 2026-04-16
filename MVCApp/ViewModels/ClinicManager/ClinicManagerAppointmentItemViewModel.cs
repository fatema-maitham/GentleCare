namespace MVCApp.ViewModels.ClinicManager
{
    public class ClinicManagerAppointmentItemViewModel
    {
        public int AppointmentId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string PatientName { get; set; } = string.Empty;
        public string? DoctorName { get; set; }

        public string StatusName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? ImpactReason { get; set; }
    }
}
