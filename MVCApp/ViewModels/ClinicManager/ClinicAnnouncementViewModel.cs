using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel for Clinic Manager announcement creation.
    // The manager can send one announcement to doctors, receptionists, patients, or all.
    public class ClinicAnnouncementViewModel
    {
        [Required(ErrorMessage = "Announcement title is required.")]
        [StringLength(120, ErrorMessage = "Title cannot be more than 120 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Announcement message is required.")]
        [StringLength(1000, ErrorMessage = "Message cannot be more than 1000 characters.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select who should receive this announcement.")]
        public string SelectedAudience { get; set; } = "All";

        public List<SelectListItem> AudienceOptions { get; set; } = new();
    }
}