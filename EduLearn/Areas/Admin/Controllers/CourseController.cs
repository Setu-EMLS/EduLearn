using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
using EduLearn.Models;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class CourseController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IFileService _files;

		public CourseController(ApplicationDbContext context, IFileService files)
		{
			_context = context;
			_files = files;
		}

		public async Task<IActionResult> Index()
		{
			var courses = await _context.Courses
				.Include(c => c.Coursecategory)
				.Include(c => c.Teacher)
				.Include(c => c.Enrollments)
				.Include(c => c.Sections).ThenInclude(s => s.Lessons)
				.OrderByDescending(c => c.CreatedAt)
				.AsSplitQuery()
				.ToListAsync();
			return View(courses);
		}

		public async Task<IActionResult> Create()
		{
			if (!await _context.CourseCategorys.AnyAsync())
			{
				TempData["Error"] = "Create at least one course category first.";
				return RedirectToAction("Index", "CourseCategory");
			}
			await LoadLookupsAsync();
			return View(new Course { IsPublished = false });
		}

		[HttpPost]
		public async Task<IActionResult> Create(Course model)
		{
			await ValidateLookupsAsync(model);
			if (model.Thumbnail != null)
			{
				var saved = await _files.SaveAsync(model.Thumbnail, "courses", UploadKind.Image);
				if (saved.Success) model.ThumbnailPath = saved.Path;
				else ModelState.AddModelError(nameof(model.Thumbnail), saved.Error!);
			}

			if (!ModelState.IsValid)
			{
				_files.Delete(model.ThumbnailPath);
				await LoadLookupsAsync();
				return View(model);
			}

			var course = new Course { CreatedAt = DateTime.Now };
			CopyFields(model, course);
			course.ThumbnailPath = model.ThumbnailPath;
			_context.Courses.Add(course);
			await _context.SaveChangesAsync();

			TempData["Success"] = "Course created. Now add sections and lessons to it.";
			return RedirectToAction("Index", "Curriculum", new { area = "Teacher", id = course.Id });
		}

		public async Task<IActionResult> Edit(int id)
		{
			var course = await _context.Courses.FindAsync(id);
			if (course == null) return NotFound();
			await LoadLookupsAsync();
			return View(course);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, Course model)
		{
			var course = await _context.Courses.FindAsync(id);
			if (course == null) return NotFound();

			await ValidateLookupsAsync(model);
			string? newThumb = null;
			if (model.Thumbnail != null)
			{
				var saved = await _files.SaveAsync(model.Thumbnail, "courses", UploadKind.Image);
				if (saved.Success) newThumb = saved.Path;
				else ModelState.AddModelError(nameof(model.Thumbnail), saved.Error!);
			}

			if (!ModelState.IsValid)
			{
				_files.Delete(newThumb);
				model.Id = id;
				model.ThumbnailPath = course.ThumbnailPath;
				await LoadLookupsAsync();
				return View(model);
			}

			CopyFields(model, course);
			if (newThumb != null)
			{
				_files.Delete(course.ThumbnailPath);
				course.ThumbnailPath = newThumb;
			}
			await _context.SaveChangesAsync();

			TempData["Success"] = "Course updated successfully.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> TogglePublish(int id)
		{
			var course = await _context.Courses.FindAsync(id);
			if (course == null) return NotFound();

			course.IsPublished = !course.IsPublished;
			await _context.SaveChangesAsync();
			TempData["Success"] = course.IsPublished
				? $"\"{course.Title}\" is now visible on the website."
				: $"\"{course.Title}\" was moved back to draft.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var course = await _context.Courses.Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.Id == id);
			if (course == null) return NotFound();

			if (course.Enrollments.Any())
			{
				TempData["Error"] = $"\"{course.Title}\" has {course.Enrollments.Count} enrollment(s) and cannot be deleted. Unpublish it instead.";
				return RedirectToAction(nameof(Index));
			}

			// Remove uploaded lesson / assignment files together with the course
			var lessonFiles = await _context.Lessons.Where(l => l.Section.CourseId == id && l.AttachmentPath != null).Select(l => l.AttachmentPath).ToListAsync();
			var assignmentFiles = await _context.Assignments.Where(a => a.CourseId == id && a.AttachmentPath != null).Select(a => a.AttachmentPath).ToListAsync();

			_context.Courses.Remove(course);
			await _context.SaveChangesAsync();

			_files.Delete(course.ThumbnailPath);
			foreach (var path in lessonFiles.Concat(assignmentFiles)) _files.Delete(path);

			TempData["Success"] = "Course deleted successfully.";
			return RedirectToAction(nameof(Index));
		}

		private static void CopyFields(Course from, Course to)
		{
			to.Title = from.Title.Trim();
			to.ShortDescription = from.ShortDescription;
			to.Description = from.Description;
			to.Price = from.Price;
			to.CategoryId = from.CategoryId;
			to.TeacherId = from.TeacherId;
			to.Level = from.Level;
			to.Duration = from.Duration;
			to.Schedule = from.Schedule;
			to.Seats = from.Seats;
			to.IsPublished = from.IsPublished;
		}

		private async Task ValidateLookupsAsync(Course model)
		{
			if (!await _context.CourseCategorys.AnyAsync(c => c.Id == model.CategoryId))
				ModelState.AddModelError(nameof(model.CategoryId), "Please select a valid category.");
			if (model.TeacherId != null && !await _context.Teachers.AnyAsync(t => t.Id == model.TeacherId))
				ModelState.AddModelError(nameof(model.TeacherId), "Please select a valid teacher.");
		}

		private async Task LoadLookupsAsync()
		{
			ViewBag.Categories = new SelectList(await _context.CourseCategorys.OrderBy(c => c.Title).ToListAsync(), "Id", "Title");
			ViewBag.Teachers = new SelectList(await _context.Teachers.OrderBy(t => t.Name).ToListAsync(), "Id", "Name");
		}
	}
}
