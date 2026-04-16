using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.ClinicManager
{
    public class ClinicManagerAppointmentsViewModel
    {
        public int? SelectedDoctorId { get; set; }
        public string? SelectedStatus { get; set; }
        public DateTime? SelectedDate { get; set; }

        public List<SelectListItem> DoctorOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();

        public List<ClinicManagerAppointmentItemViewModel> Appointments { get; set; } = new();
    }
}
