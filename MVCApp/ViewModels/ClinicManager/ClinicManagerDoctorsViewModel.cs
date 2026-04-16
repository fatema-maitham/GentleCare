namespace MVCApp.ViewModels.ClinicManager
{
    public class ClinicManagerDoctorsViewModel
    {
        public string? Search { get; set; }
        public List<DoctorListItemViewModel> Doctors { get; set; } = new();
    }
}
