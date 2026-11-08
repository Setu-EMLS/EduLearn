using Microsoft.AspNetCore.Mvc;

namespace EduLearn.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
