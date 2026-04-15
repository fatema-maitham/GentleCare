using System.Collections.Generic;

namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistDashboardViewModel
    {
        public int TodayTotalAppointments { get; set; }
        public int TodayConfirmedCount { get; set; }
        public int TodayCheckedInCount { get; set; }
        public int TodayInProgressCount { get; set; }
        public int TodayCompletedCount { get; set; }

        public List<ReceptionistAppointmentListItemViewModel> TodayAppointments { get; set; } = new();
    }
}