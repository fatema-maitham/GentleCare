using WebAPI.Models;

namespace MVCApp.ViewModels.Doctor
{
    public class DoctorScheduleViewModel
    {
        public string DoctorFullName { get; set; } = string.Empty;

        public List<DoctorSchedule> Schedules { get; set; } = new();
        public List<DoctorLeave> Leaves { get; set; } = new();
    }
}