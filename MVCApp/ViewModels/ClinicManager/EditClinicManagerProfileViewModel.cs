using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.ClinicManager
{
    public class EditClinicManagerProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? CurrentProfilePicture { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}