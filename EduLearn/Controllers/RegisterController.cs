using Microsoft.AspNetCore.Mvc;
using EduLearn.Infrastructure;
using EduLearn.Models.ViewModels;

namespace EduLearn.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Index(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return Redirect(User.DashboardUrl());

            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }
    }
}
