namespace MVCApp.ViewModels.ClinicManager
{
    public class ManageDoctorLeavesViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public List<DoctorLeaveItemViewModel> Leaves { get; set; } = new();
    }
}
