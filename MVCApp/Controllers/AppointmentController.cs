using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Controllers
{
    public class AppointmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
