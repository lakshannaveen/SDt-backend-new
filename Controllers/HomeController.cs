using Microsoft.AspNetCore.Mvc;

namespace YourProjectNamespace.Controllers
{
    public class HomeController : Controller
    {
        // Default action method for Home page
        public IActionResult Index()
        {
            return View();
        }

        // About action method
        public IActionResult About()
        {
            return View();
        }

        // Contact action method
        public IActionResult Contact()
        {
            return View();
        }
    }
}
