using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.ClinicManager
{
    // Page model for Clinic Manager account activation/deactivation.
    public class ClinicManagerUserAccountsViewModel
    {
        public string? SearchTerm { get; set; }

        public string? SelectedRole { get; set; }

        public bool? IsActive { get; set; }

        public List<SelectListItem> RoleOptions { get; set; } = new();

        public List<SelectListItem> StatusOptions { get; set; } = new();

        public List<ClinicManagerUserAccountItemViewModel> Users { get; set; } = new();
    }

    public class ClinicManagerUserAccountItemViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public string StatusText => IsActive ? "Active" : "Inactive";

        public string CreatedAtDisplay => CreatedAt == default
            ? "-"
            : CreatedAt.ToString("dd MMM yyyy");
    }
}