using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
