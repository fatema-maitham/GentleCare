using ClinicReportingApp.Models;
using ClinicReportingApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicReportingApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IClinicApiService _api;
        private readonly ITokenService _tokenService;

        public AuthController(IClinicApiService api, ITokenService tokenService)
        {
            _api = api;
            _tokenService = tokenService;
        }

        // GET /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_tokenService.IsAuthenticated(HttpContext))
                return RedirectToAction("Index", "Dashboard");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _api.LoginAsync(model.Email, model.Password);
            Console.WriteLine($">>> result null: {result == null}");
            Console.WriteLine($">>> Role: '{result?.Role}' | Length: {result?.Role?.Length}");

            if (result == null)
            {
                ModelState.AddModelError("", "Invalid credentials or insufficient permissions.");
                return View(model);
            }

            // Only allow Clinic Manager role
            if (!result.Role.Equals("ClinicManager", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Access denied. This application is restricted to Clinic Managers only.");
                return View(model);
            }

            _tokenService.StoreToken(HttpContext, result.Token, result.FullName, result.Role);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        // POST /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _tokenService.ClearToken(HttpContext);
            return RedirectToAction("Login");
        }
    }
}
