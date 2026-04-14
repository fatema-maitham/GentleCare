using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Controllers
{
    public class PatientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
