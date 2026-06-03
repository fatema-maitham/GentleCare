using ClinicReportingApp.Models;
using ClinicReportingApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicReportingApp.Controllers
{
    // the Auth controller
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

            try
            {
                var result = await _api.LoginAsync(model.Email, model.Password);

                if (result == null)
                {
                    ModelState.AddModelError("", "Invalid credentials or insufficient permissions.");
                    return View(model);
                }

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
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "WebAPI service is currently unavailable.");
                ModelState.AddModelError("", "Please ensure the WebAPI is running, or try again later.");

                return View(model);
            }
        }
        //GET /Auth/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Access Denied";
            return View();
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
