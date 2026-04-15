namespace MVCApp.ViewModels.ClinicManager
{
    public class ManageDoctorScheduleViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public List<DoctorScheduleItemViewModel> Schedules { get; set; } = new();
    }
}
