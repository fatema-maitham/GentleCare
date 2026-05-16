using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Patient
{
    public class PatientEditProfileViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string CPRNumber { get; set; } = string.Empty;

        public string ReferenceNumber { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string? BloodType { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(30)]
        public string? EmergencyContactPhone { get; set; }

        public string? CurrentProfilePicture { get; set; }

        public IFormFile? ProfileImageFile { get; set; }
    }
}