namespace MVCApp.ViewModels.ClinicManager
{
    public class AppointmentImpactViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<ClinicManagerAppointmentItemViewModel> ImpactedAppointments { get; set; } = new();
    }
}
