using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class ProfileController : StudentAreaController
	{
		private readonly IFileService _files;

		public ProfileController(ApplicationDbContext db, IAccountService accounts, IFileService files) : base(db, accounts)
		{
			_files = files;
		}

		public IActionResult Index()
		{
			var s = CurrentStudent;
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
		public async Task<IActionResult> Index(PersonFormViewModel model)
		{
			var student = CurrentStudent;

			// Email is the login and is changed by the office only
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
				model.Email = student.Email;
				model.Code = student.StudentID;
				model.PhotoPath = student.PhotoPath;
				return View(model);
			}

			student.Name = model.Name.Trim();
			student.Contact = model.Contact;
			student.Address = model.Address;
			student.FatherName = model.FatherName;
			student.MotherName = model.MotherName;
			student.DateOfBirth = model.DateOfBirth;
			student.Description = model.Description;
			if (photo != null)
			{
				_files.Delete(student.PhotoPath);
				student.PhotoPath = photo;
			}

			var user = await Db.Tbl_Users.FirstOrDefaultAsync(u => u.UserId == student.UserId);
			if (user != null)
			{
				user.FullName = student.Name;
				user.MobileNo = student.Contact;
			}
			await Db.SaveChangesAsync();

			TempData["Success"] = "Profile updated.";
			return RedirectToAction(nameof(Index));
		}
	}
}
