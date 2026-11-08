using Microsoft.AspNetCore.Mvc;

namespace EduLearn.Controllers
{
    public class StudentsFeedbackController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
