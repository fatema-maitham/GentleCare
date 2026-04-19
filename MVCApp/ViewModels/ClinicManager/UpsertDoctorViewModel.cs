using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.ClinicManager
{
    public class UpsertDoctorViewModel
    {
        public int? DoctorId { get; set; }
        public string? UserId { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        public string? Bio { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
