using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.ClinicManager
{
    public class UpsertDoctorScheduleViewModel
    {
        public int? DoctorScheduleId { get; set; }
        public int DoctorId { get; set; }

        [Display(Name = "Day")]
        public DayOfWeek DayOfWeek { get; set; }

        [Display(Name = "Start Time")]
        public TimeOnly StartTime { get; set; }

        [Display(Name = "End Time")]
        public TimeOnly EndTime { get; set; }

        [Display(Name = "Slot Duration (Minutes)")]
        public int SlotDurationMinutes { get; set; } = 30;
    }
}
