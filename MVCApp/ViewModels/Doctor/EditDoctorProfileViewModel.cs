using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Doctor
{
    public class EditDoctorProfileViewModel
    {
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        public string? CurrentProfilePicture { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}