using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class TeacherController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IAccountService _accounts;
		private readonly IFileService _files;

		public TeacherController(ApplicationDbContext context, IAccountService accounts, IFileService files)
		{
			_context = context;
			_accounts = accounts;
			_files = files;
		}

		public async Task<IActionResult> Index()
		{
			var teachers = await _context.Teachers
				.Include(t => t.User)
				.Include(t => t.Courses)
				.OrderBy(t => t.Name)
				.ToListAsync();
			return View(teachers);
		}

		public IActionResult Create()
		{
			return View(new PersonFormViewModel());
		}

		[HttpPost]
		public async Task<IActionResult> Create(PersonFormViewModel model)
		{
			if (string.IsNullOrWhiteSpace(model.Password))
				ModelState.AddModelError(nameof(model.Password), "Password is required for a new teacher.");
			if (ModelState.IsValid && _accounts.EmailExists(model.Email))
				ModelState.AddModelError(nameof(model.Email), "This email is already used by another account.");

			string? photo = null;
			if (ModelState.IsValid && model.Photo != null)
			{
				var saved = await _files.SaveAsync(model.Photo, "people", UploadKind.Image);
				if (saved.Success) photo = saved.Path;
				else ModelState.AddModelError(nameof(model.Photo), saved.Error!);
			}

			if (!ModelState.IsValid) return View(model);

			var teacher = await _accounts.CreateTeacherAsync(model.Name, model.Email, model.Contact, model.Password!, model.Designation);
			CopyProfile(model, teacher);
			teacher.PhotoPath = photo;
			await _context.SaveChangesAsync();

			TempData["Success"] = $"Teacher {teacher.Name} created (ID {teacher.TeacherID}). They can log in with {teacher.Email}.";
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Edit(int id)
		{
			var t = await _context.Teachers.FindAsync(id);
			if (t == null) return NotFound();

			return View(new PersonFormViewModel
			{
				Id = t.Id,
				Code = t.TeacherID,
				Name = t.Name,
				Email = t.Email,
				Contact = t.Contact,
				Designation = t.Designation,
				Address = t.Address,
				FatherName = t.FatherName,
				MotherName = t.MotherName,
				DateOfBirth = t.DateOfBirth,
				Description = t.Description,
				PhotoPath = t.PhotoPath
			});
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, PersonFormViewModel model)
		{
			var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
			if (teacher == null) return NotFound();

			if (ModelState.IsValid && _accounts.EmailExists(model.Email, teacher.UserId))
				ModelState.AddModelError(nameof(model.Email), "This email is already used by another account.");

			string? photo = null;
			if (ModelState.IsValid && model.Photo != null)
			{
				var saved = await _files.SaveAsync(model.Photo, "people", UploadKind.Image);
				if (saved.Success) photo = saved.Path;
				else ModelState.AddModelError(nameof(model.Photo), saved.Error!);
			}

			if (!ModelState.IsValid)
			{
				model.Id = id;
				model.Code = teacher.TeacherID;
				model.PhotoPath = teacher.PhotoPath;
				return View(model);
			}

			teacher.Name = model.Name.Trim();
			teacher.Email = model.Email.Trim();
			teacher.Contact = model.Contact;
			CopyProfile(model, teacher);
			if (photo != null)
			{
				_files.Delete(teacher.PhotoPath);
				teacher.PhotoPath = photo;
			}

			if (teacher.User != null)
			{
				teacher.User.FullName = teacher.Name;
				teacher.User.Email = teacher.Email;
				teacher.User.MobileNo = teacher.Contact;
				if (!string.IsNullOrWhiteSpace(model.Password))
					_accounts.SetPassword(teacher.User, model.Password);
			}

			await _context.SaveChangesAsync();
			TempData["Success"] = "Teacher updated successfully.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> ToggleActive(int id)
		{
			var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
			if (teacher?.User == null) return NotFound();

			teacher.User.IsActive = !teacher.User.IsActive;
			await _context.SaveChangesAsync();
			TempData["Success"] = $"{teacher.Name} can {(teacher.User.IsActive ? "now" : "no longer")} log in.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var teacher = await _context.Teachers.Include(t => t.User).Include(t => t.Courses).FirstOrDefaultAsync(t => t.Id == id);
			if (teacher == null) return NotFound();

			if (teacher.Courses.Any())
			{
				TempData["Error"] = $"{teacher.Name} is assigned to {teacher.Courses.Count} course(s). Reassign those courses first, or deactivate the account instead.";
				return RedirectToAction(nameof(Index));
			}

			var user = teacher.User;
			_context.Teachers.Remove(teacher);
			if (user != null) _context.Tbl_Users.Remove(user);
			await _context.SaveChangesAsync();
			_files.Delete(teacher.PhotoPath);

			TempData["Success"] = "Teacher deleted.";
			return RedirectToAction(nameof(Index));
		}

		private static void CopyProfile(PersonFormViewModel model, TeacherEntity teacher)
		{
			teacher.Designation = model.Designation;
			teacher.Address = model.Address;
			teacher.FatherName = model.FatherName;
			teacher.MotherName = model.MotherName;
			teacher.DateOfBirth = model.DateOfBirth;
			teacher.Description = model.Description;
		}
	}
}
