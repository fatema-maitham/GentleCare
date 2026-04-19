namespace MVCApp.ViewModels.ClinicManager
{
    public class ClinicManagerDashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int AppointmentsToday { get; set; }
        public int RequestedAppointments { get; set; }
        public int DoctorsOnLeaveToday { get; set; }
    }
}
