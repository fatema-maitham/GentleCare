using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistAppointmentsPageViewModel
    {
        public string? SearchText { get; set; }
        public DateTime? SelectedDate { get; set; }
        public int? SelectedStatusId { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<ReceptionistAppointmentListItemViewModel> Appointments { get; set; } = new();
    }
}