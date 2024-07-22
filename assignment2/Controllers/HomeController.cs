using Microsoft.AspNetCore.Mvc;

namespace Assignment2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Welcome";
            ViewData["Message"] = "Welcome to the Asssignment 2 application.";
            ViewData["Comment"] = "My name is Marcos Astudillo \nAnd feel free to leave any comments or feedback about the assignments or course.";
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
