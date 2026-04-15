using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            TokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var validRoles = new[] { "Patient", "Doctor",
                "Receptionist", "ClinicManager" };
            if (!validRoles.Contains(dto.Role))
                return BadRequest(new { message = "Invalid role specified." });

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already in use." });

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
     
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                return Unauthorized(new { message = "Invalid credentials." });

    
            var passwordValid = await _userManager
                .CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Unauthorized(new { message = "Invalid credentials." });


            var roles = await _userManager.GetRolesAsync(user);

         
            var token = _tokenService.GenerateToken(user, roles);

            return Ok(new AuthResponseDTO
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "",
                Expiry = DateTime.UtcNow.AddDays(7)
            });
        }

    }
}