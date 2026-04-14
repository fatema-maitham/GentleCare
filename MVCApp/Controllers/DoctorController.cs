using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
