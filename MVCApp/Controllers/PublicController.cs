using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        [HttpGet]
        public IActionResult Lookup()
        {
            ViewData["Title"] = "Public Lookup";
            return View();
        }
    }
}