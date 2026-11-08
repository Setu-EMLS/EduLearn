using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;
using Project_Management.Models;
namespace EduLearn.Controllers
{
    public class HomeController : Controller
    {
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IUserService _userService;
        private readonly IIdentityService _identityService;
        private readonly IAccountService _accountService;
        public HomeController(ILogger<HomeController> logger,
                      ApplicationDbContext context,
                      IUserService userService,
                      IIdentityService identityService,
                      IAccountService accountService)
		{
			_logger = logger;
			_userService = userService;
            _identityService = identityService;
            _accountService = accountService;
            _context = context;
		}

		public async Task<IActionResult> Index()
        {
            ViewData["Courses"] = await _context.Courses
                .Where(c => c.IsPublished)
                .Include(c => c.Coursecategory)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                .AsSplitQuery()
                .OrderByDescending(c => c.CreatedAt)
                .Take(4)
                .ToListAsync();
            ViewData["Mentors"] = await _context.Teachers
                .Where(t => t.User == null || t.User.IsActive)
                .OrderBy(t => t.Name)
                .Take(4)
                .ToListAsync();
            // "Watch a free lesson" in the hero opens the first free preview lesson, if any
            ViewData["PreviewLessonId"] = await _context.Lessons
                .Where(l => l.IsPreview && l.Section.Course.IsPublished)
                .OrderBy(l => l.Id)
                .Select(l => (int?)l.Id)
                .FirstOrDefaultAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
		//=================================== login ===================================//
		public IActionResult Login(string? returnUrl = null)
		{
			return RedirectToAction("Index", "Login", new { returnUrl });
		}

		[HttpPost]
		public async Task<IActionResult> Login(Login model, string? returnUrl = null)
		{
		    ViewBag.ReturnUrl = returnUrl;
		    if (ModelState.IsValid)
		    {
		        var user = _userService.GetUserByEmail(model.Email.Trim());
		        if (user != null)
		        {
		            if (_userService.VerifyPassword(model.Password, user.Password, user.Salt))
		            {
		                if (!user.IsActive)
		                {
		                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact the office.");
		                    return View("~/Views/Login/Index.cshtml", model);
		                }

		                await _identityService.signInUser(user);

		                CookieOptions option = new CookieOptions
		                {
		                    Expires = DateTime.Now.AddDays(1),
		                    HttpOnly = true,
		                    Secure = Request.IsHttps,
		                    SameSite = SameSiteMode.Lax
		                };

		                Response.Cookies.Append("UserEmail", user.Email, option);
		                Response.Cookies.Append("UserRole", user.RoleId.ToString(), option);

		                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
		                    return LocalRedirect(returnUrl);

		                // Redirect based on role
		                if (user.RoleId == RoleIds.Admin) return RedirectToAction("Index", "Home", new { Area = "Admin" });
		                else if (user.RoleId == RoleIds.Teacher) return RedirectToAction("Index", "Home", new { Area = "Teacher" });
		                else if (user.RoleId == RoleIds.Student) return RedirectToAction("Index", "Home", new { Area = "Student" });

		                return RedirectToAction("Index", "Home");
		            }
		            else
		            {
		                ModelState.AddModelError("Password", "Incorrect password. Please try again.");
		            }
		        }
		        else
		        {
		            ModelState.AddModelError("Email", "User not found. Please check your email.");
		        }
		    }

		    return View("~/Views/Login/Index.cshtml", model);
		}


		//============================== Register ==============================//
		public IActionResult Register(string? returnUrl = null)
		{
			return RedirectToAction("Index", "Register", new { returnUrl });
		}

		// Public sign-up always creates a Student account
		[HttpPost]
		public async Task<IActionResult> SignUp(RegisterViewModel model)
		{
			if (ModelState.IsValid && _accountService.EmailExists(model.Email))
			{
				ModelState.AddModelError("Email", "Email is already registered.");
			}

			if (!ModelState.IsValid)
			{
				return View("~/Views/Register/Index.cshtml", model);
			}

			var student = await _accountService.CreateStudentAsync(model.FullName, model.Email, model.MobileNo, model.Password);
			var user = await _context.Tbl_Users.FirstAsync(u => u.UserId == student.UserId);
			await _identityService.signInUser(user);

			TempData["Success"] = $"Welcome {user.FullName}! Your student ID is {student.StudentID}.";
			if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
				return LocalRedirect(model.ReturnUrl);
			return RedirectToAction("Index", "Home", new { Area = "Student" });
		}

		//========================== LogOut ======================//
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(); // Signs out the user
			Response.Cookies.Delete("UserEmail");
			Response.Cookies.Delete("UserRole");
			return RedirectToAction("Index", "Login");
		}

		public IActionResult AccessDenied()
		{
			return View();
		}

		//========================== Change Password =====================//
		[Authorize]
		[HttpGet]
		public IActionResult ChangePassword()
		{
			return View(new ChangePasswordViewModel());
		}

		[Authorize]
		[HttpPost]
		public IActionResult ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var email = User.GetEmail();
			if (string.IsNullOrEmpty(email))
				return Unauthorized();

			var success = _userService.ChangePassword(email, model.CurrentPassword, model.NewPassword);
			if (!success)
			{
				ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
				return View(model);
			}

			TempData["Success"] = "Password changed successfully.";
			return RedirectToAction(nameof(ChangePassword));
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
