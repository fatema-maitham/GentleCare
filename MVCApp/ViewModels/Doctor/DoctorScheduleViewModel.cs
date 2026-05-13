using WebAPI.Models;

namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Doctor Schedule page.
    // It shows the logged-in doctor's weekly working hours and leave periods.
    public class DoctorScheduleViewModel
    {
        // Doctor name displayed at the top of the schedule page.
        public string DoctorFullName { get; set; } = string.Empty;

        // Weekly working schedule assigned to the doctor.
        public List<DoctorSchedule> Schedules { get; set; } = new();

        // Approved leave periods for the doctor.
        public List<DoctorLeave> Leaves { get; set; } = new();

        // Display helpers for the view.
        public bool HasSchedules => Schedules.Any();
        public bool HasLeaves => Leaves.Any();

        // Used to show the total number of working days configured.
        public int WorkingDaysCount => Schedules
            .Select(s => s.DayOfWeek)
            .Distinct()
            .Count();
    }
}