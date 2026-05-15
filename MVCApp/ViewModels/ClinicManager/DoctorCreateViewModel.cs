using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to create a new doctor account and profile.
    public class DoctorCreateViewModel
    {
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

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirm password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

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

        // Used to fill the specialization checkboxes/dropdown in the CreateDoctor view.
        public List<SelectListItem> SpecializationOptions { get; set; } = new();
    }
}