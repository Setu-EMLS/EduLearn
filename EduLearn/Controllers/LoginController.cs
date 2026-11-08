using Microsoft.AspNetCore.Mvc;
using EduLearn.Infrastructure;

namespace EduLearn.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return Redirect(User.DashboardUrl());

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
