using Microsoft.AspNetCore.Mvc;

namespace EduLearn.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
