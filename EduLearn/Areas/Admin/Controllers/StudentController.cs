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
	public class StudentController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IAccountService _accounts;
		private readonly IFileService _files;
		private readonly ILearningService _learning;

		public StudentController(ApplicationDbContext context, IAccountService accounts, IFileService files, ILearningService learning)
		{
			_context = context;
			_accounts = accounts;
			_files = files;
			_learning = learning;
		}

		public async Task<IActionResult> Index()
		{
			var students = await _context.Students
				.Include(s => s.User)
				.Include(s => s.Enrollments)
				.OrderByDescending(s => s.Id)
				.ToListAsync();
			return View(students);
		}

		public async Task<IActionResult> Details(int id)
		{
			var student = await _context.Students
				.Include(s => s.User)
				.Include(s => s.Enrollments).ThenInclude(e => e.Course)
				.FirstOrDefaultAsync(s => s.Id == id);
			if (student == null) return NotFound();

			ViewBag.Progress = await _learning.GetProgressAsync(student.Enrollments.Select(e => e.Id));
			ViewBag.Submissions = await _context.AssignmentSubmissions
				.Include(s => s.Assignment).ThenInclude(a => a.Course)
				.Where(s => s.StudentId == id)
				.OrderByDescending(s => s.SubmittedAt)
				.ToListAsync();
			ViewBag.Attempts = await _context.QuizAttempts
				.Include(a => a.Quiz).ThenInclude(q => q.Course)
				.Where(a => a.StudentId == id && a.SubmittedAt != null)
				.OrderByDescending(a => a.SubmittedAt)
				.ToListAsync();
			return View(student);
		}

		public IActionResult Create()
		{
			return View(new PersonFormViewModel());
		}

		[HttpPost]
		public async Task<IActionResult> Create(PersonFormViewModel model)
		{
			if (string.IsNullOrWhiteSpace(model.Password))
				ModelState.AddModelError(nameof(model.Password), "Password is required for a new student.");
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

			var student = await _accounts.CreateStudentAsync(model.Name, model.Email, model.Contact, model.Password!);
			CopyProfile(model, student);
			student.PhotoPath = photo;
			await _context.SaveChangesAsync();

			TempData["Success"] = $"Student {student.Name} created (ID {student.StudentID}).";
			return RedirectToAction(nameof(Details), new { id = student.Id });
		}

		public async Task<IActionResult> Edit(int id)
		{
			var s = await _context.Students.FindAsync(id);
			if (s == null) return NotFound();

			return View(new PersonFormViewModel
			{
				Id = s.Id,
				Code = s.StudentID,
				Name = s.Name,
				Email = s.Email,
				Contact = s.Contact,
				Address = s.Address,
				FatherName = s.FatherName,
				MotherName = s.MotherName,
				DateOfBirth = s.DateOfBirth,
				Description = s.Description,
				PhotoPath = s.PhotoPath
			});
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, PersonFormViewModel model)
		{
			var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
			if (student == null) return NotFound();

			if (ModelState.IsValid && _accounts.EmailExists(model.Email, student.UserId))
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
				model.Code = student.StudentID;
				model.PhotoPath = student.PhotoPath;
				return View(model);
			}

			student.Name = model.Name.Trim();
			student.Email = model.Email.Trim();
			student.Contact = model.Contact;
			CopyProfile(model, student);
			if (photo != null)
			{
				_files.Delete(student.PhotoPath);
				student.PhotoPath = photo;
			}

			if (student.User != null)
			{
				student.User.FullName = student.Name;
				student.User.Email = student.Email;
				student.User.MobileNo = student.Contact;
				if (!string.IsNullOrWhiteSpace(model.Password))
					_accounts.SetPassword(student.User, model.Password);
			}

			await _context.SaveChangesAsync();
			TempData["Success"] = "Student updated successfully.";
			return RedirectToAction(nameof(Details), new { id });
		}

		[HttpPost]
		public async Task<IActionResult> ToggleActive(int id)
		{
			var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
			if (student?.User == null) return NotFound();

			student.User.IsActive = !student.User.IsActive;
			await _context.SaveChangesAsync();
			TempData["Success"] = $"{student.Name} can {(student.User.IsActive ? "now" : "no longer")} log in.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var student = await _context.Students.Include(s => s.User).Include(s => s.Enrollments).FirstOrDefaultAsync(s => s.Id == id);
			if (student == null) return NotFound();

			if (student.Enrollments.Any())
			{
				TempData["Error"] = $"{student.Name} has {student.Enrollments.Count} enrollment(s). Deactivate the account instead of deleting it.";
				return RedirectToAction(nameof(Index));
			}

			var user = student.User;
			_context.Students.Remove(student);
			if (user != null) _context.Tbl_Users.Remove(user);
			await _context.SaveChangesAsync();
			_files.Delete(student.PhotoPath);

			TempData["Success"] = "Student deleted.";
			return RedirectToAction(nameof(Index));
		}

		private static void CopyProfile(PersonFormViewModel model, StudentEntity student)
		{
			student.Address = model.Address;
			student.FatherName = model.FatherName;
			student.MotherName = model.MotherName;
			student.DateOfBirth = model.DateOfBirth;
			student.Description = model.Description;
		}
	}
}
