using System.Collections.Generic;

namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistPatientSearchViewModel
    {
        public string? SearchText { get; set; }

        public List<ReceptionistPatientSearchResultViewModel> Patients { get; set; } = new();
    }
}