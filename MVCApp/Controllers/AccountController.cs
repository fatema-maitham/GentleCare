using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCApp.ViewModels;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["Title"] = "Login";
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["Title"] = "Login";
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser? user;

            if (model.UsernameOrEmail.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(model.UsernameOrEmail);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.UsernameOrEmail);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "This account is inactive.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, "Patient"))
            {
                return RedirectToAction("Profile", "Patient");
            }

            if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                return RedirectToAction("Dashboard", "Doctor");
            }

            if (await _userManager.IsInRoleAsync(user, "Receptionist"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (await _userManager.IsInRoleAsync(user, "ClinicManager"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return RedirectToAction("Lookup", "Public");
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Register";
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["Title"] = "Register";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.DateOfBirth > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Date of birth cannot be in the future.");
                return View(model);
            }

            var existingByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingByUsername != null)
            {
                ModelState.AddModelError(nameof(model.Username), "Username already exists.");
                return View(model);
            }

            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email already exists.");
                return View(model);
            }

            var existingByCpr = await _context.Patients
                .AnyAsync(p => p.CPRNumber == model.CPRNumber);

            if (existingByCpr)
            {
                ModelState.AddModelError(nameof(model.CPRNumber), "CPR number already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Username,
                Email = model.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Patient");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            string referenceNumber = await GenerateUniqueReferenceNumberAsync();

            var patient = new Patient
            {
                UserId = user.Id,
                CPRNumber = model.CPRNumber,
                ReferenceNumber = referenceNumber,
                DateOfBirth = model.DateOfBirth,
                BloodType = null,
                Address = null,
                EmergencyContactName = null,
                EmergencyContactPhone = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            try
            {
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, "Registration failed while creating patient profile.");
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Profile", "Patient");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Access Denied";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Lookup", "Public");
        }

        private async Task<string> GenerateUniqueReferenceNumberAsync()
        {
            string referenceNumber;

            do
            {
                referenceNumber = "REF-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
            }
            while (await _context.Patients.AnyAsync(p => p.ReferenceNumber == referenceNumber));

            return referenceNumber;
        }
    }
}