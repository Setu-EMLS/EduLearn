using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Teacher.Controllers
{
	[Authorize(Roles = "Teacher")]
	public class ProfileController : TeacherAreaController
	{
		private readonly IFileService _files;

		public ProfileController(ApplicationDbContext db, IFileService files) : base(db)
		{
			_files = files;
		}

		public async Task<IActionResult> Index()
		{
			var t = await CurrentTeacherAsync();
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
				DateOfBirth = t.DateOfBirth,
				Description = t.Description,
				PhotoPath = t.PhotoPath
			});
		}

		[HttpPost]
		public async Task<IActionResult> Index(PersonFormViewModel model)
		{
			var teacher = await CurrentTeacherAsync();
			if (teacher == null) return NotFound();
			var user = await Db.Tbl_Users.FirstOrDefaultAsync(u => u.UserId == teacher.UserId);

			// Email is the login and is changed by the admin only
			ModelState.Remove(nameof(model.Email));
			string? photo = null;
			if (ModelState.IsValid && model.Photo != null)
			{
				var saved = await _files.SaveAsync(model.Photo, "people", UploadKind.Image);
				if (saved.Success) photo = saved.Path;
				else ModelState.AddModelError(nameof(model.Photo), saved.Error!);
			}

			if (!ModelState.IsValid)
			{
				model.Email = teacher.Email;
				model.Code = teacher.TeacherID;
				model.PhotoPath = teacher.PhotoPath;
				return View(model);
			}

			teacher.Name = model.Name.Trim();
			teacher.Contact = model.Contact;
			teacher.Designation = model.Designation;
			teacher.Address = model.Address;
			teacher.DateOfBirth = model.DateOfBirth;
			teacher.Description = model.Description;
			if (photo != null)
			{
				_files.Delete(teacher.PhotoPath);
				teacher.PhotoPath = photo;
			}
			if (user != null)
			{
				user.FullName = teacher.Name;
				user.MobileNo = teacher.Contact;
			}
			await Db.SaveChangesAsync();

			TempData["Success"] = "Profile updated. Your new name shows after you log in again.";
			return RedirectToAction(nameof(Index));
		}
	}
}
