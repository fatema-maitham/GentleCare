using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to edit an existing doctor account and profile.
    public class DoctorEditViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot be longer than 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "License number is required.")]
        [StringLength(50, ErrorMessage = "License number cannot be longer than 50 characters.")]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio cannot be longer than 500 characters.")]
        public string? Bio { get; set; }

        [Display(Name = "Active Doctor")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Specializations")]
        public List<int> SelectedSpecializationIds { get; set; } = new();

        // Used to fill the specialization checkboxes/dropdown in the EditDoctor view.
        public List<SelectListItem> SpecializationOptions { get; set; } = new();

        // Optional password reset.
        // Leave empty if the Clinic Manager does not want to change the doctor's password.
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "New password and confirm password do not match.")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }
    }
}