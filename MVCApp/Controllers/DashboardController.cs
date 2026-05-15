using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Controllers
{
    // Sends each logged-in user to the correct role dashboard.
    // Guests are sent to the public appointment lookup page.
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Lookup", "Public");
            }

            if (User.IsInRole("Doctor"))
            {
                return RedirectToAction("Dashboard", "Doctor");
            }

            if (User.IsInRole("ClinicManager"))
            {
                return RedirectToAction("Dashboard", "ClinicManager");
            }

            if (User.IsInRole("Receptionist"))
            {
                return RedirectToAction("Index", "Receptionist");
            }

            if (User.IsInRole("Patient"))
            {
                return RedirectToAction("Dashboard", "Patient");
            }

            return RedirectToAction("AccessDenied", "Account");
        }
    }
}