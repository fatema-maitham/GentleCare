using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistEditProfileViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        public string? CurrentProfilePicture { get; set; }

        public IFormFile? ProfileImageFile { get; set; }
    }
}