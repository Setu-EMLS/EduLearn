using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Admin.Controllers
{
	// Manages the administrator logins (teachers and students have their own pages)
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class ManageUserController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IAccountService _accounts;

		public ManageUserController(ApplicationDbContext context, IAccountService accounts)
		{
			_context = context;
			_accounts = accounts;
		}

		public async Task<IActionResult> Index()
		{
			var admins = await _context.Tbl_Users
				.Where(u => u.RoleId == RoleIds.Admin)
				.OrderBy(u => u.FullName)
				.ToListAsync();
			return View(admins);
		}

		[HttpPost]
		public async Task<IActionResult> Create(PersonFormViewModel model)
		{
			if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
				ModelState.AddModelError(nameof(model.Password), "Password must be at least 6 characters long.");
			if (ModelState.IsValid && _accounts.EmailExists(model.Email))
				ModelState.AddModelError(nameof(model.Email), "This email is already used by another account.");

			if (!ModelState.IsValid)
			{
				TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
				return RedirectToAction(nameof(Index));
			}

			await _accounts.CreateAdminAsync(model.Name, model.Email, model.Contact, model.Password!);
			TempData["Success"] = $"Admin account {model.Email} created.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> ResetPassword(int id, string newPassword)
		{
			var user = await _context.Tbl_Users.FirstOrDefaultAsync(u => u.UserId == id && u.RoleId == RoleIds.Admin);
			if (user == null) return NotFound();

			if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
			{
				TempData["Error"] = "Password must be at least 6 characters long.";
				return RedirectToAction(nameof(Index));
			}

			_accounts.SetPassword(user, newPassword);
			await _context.SaveChangesAsync();
			TempData["Success"] = $"Password updated for {user.Email}.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> ToggleActive(int id)
		{
			var user = await _context.Tbl_Users.FirstOrDefaultAsync(u => u.UserId == id && u.RoleId == RoleIds.Admin);
			if (user == null) return NotFound();

			if (user.UserId == User.GetUserId())
			{
				TempData["Error"] = "You cannot deactivate your own account.";
				return RedirectToAction(nameof(Index));
			}

			user.IsActive = !user.IsActive;
			await _context.SaveChangesAsync();
			TempData["Success"] = $"{user.Email} is now {(user.IsActive ? "active" : "inactive")}.";
			return RedirectToAction(nameof(Index));
		}
	}
}
