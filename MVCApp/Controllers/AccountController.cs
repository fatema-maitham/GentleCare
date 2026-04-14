using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Register";
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Access Denied";
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}