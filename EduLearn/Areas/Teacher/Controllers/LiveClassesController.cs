using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Areas.Teacher.Controllers
{
	public class LiveClassesController : TeacherAreaController
	{
		public LiveClassesController(ApplicationDbContext db) : base(db)
		{
		}

		public async Task<IActionResult> Index()
		{
			var courseIds = await ManageableCourseIdsAsync();
			var classes = await Db.LiveClasses
				.Include(l => l.Course)
				.Where(l => courseIds.Contains(l.CourseId))
				.OrderByDescending(l => l.StartsAt)
				.ToListAsync();
			return View(classes);
		}

		public async Task<IActionResult> Create(int? courseId = null)
		{
			ViewBag.Courses = await CourseSelectListAsync(courseId);
			return View("Form", new LiveClass { CourseId = courseId ?? 0 });
		}

		[HttpPost]
		public async Task<IActionResult> Create(LiveClass model)
		{
			await ValidateAsync(model);
			if (!ModelState.IsValid)
			{
				ViewBag.Courses = await CourseSelectListAsync(model.CourseId);
				return View("Form", model);
			}

			var live = new LiveClass();
			Copy(model, live);
			Db.LiveClasses.Add(live);
			await Db.SaveChangesAsync();
			TempData["Success"] = "Live class scheduled. Enrolled students will see it in their schedule.";
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Edit(int id)
		{
			var live = await Db.LiveClasses.FindAsync(id);
			if (live == null || !await CanManageCourseAsync(live.CourseId)) return NotFound();
			ViewBag.Courses = await CourseSelectListAsync(live.CourseId);
			return View("Form", live);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, LiveClass model)
		{
			var live = await Db.LiveClasses.FindAsync(id);
			if (live == null || !await CanManageCourseAsync(live.CourseId)) return NotFound();

			await ValidateAsync(model);
			if (!ModelState.IsValid)
			{
				model.Id = id;
				ViewBag.Courses = await CourseSelectListAsync(model.CourseId);
				return View("Form", model);
			}

			Copy(model, live);
			await Db.SaveChangesAsync();
			TempData["Success"] = "Live class updated.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var live = await Db.LiveClasses.FindAsync(id);
			if (live == null || !await CanManageCourseAsync(live.CourseId)) return NotFound();
			Db.LiveClasses.Remove(live);
			await Db.SaveChangesAsync();
			TempData["Success"] = "Live class removed.";
			return RedirectToAction(nameof(Index));
		}

		private async Task ValidateAsync(LiveClass model)
		{
			if (!await CanManageCourseAsync(model.CourseId))
				ModelState.AddModelError(nameof(model.CourseId), "Please select one of your courses.");
			if (!string.IsNullOrWhiteSpace(model.MeetingUrl) && UiHelpers.SafeUrl(model.MeetingUrl) == null)
				ModelState.AddModelError(nameof(model.MeetingUrl), "Enter a full link starting with https://");
		}

		private static void Copy(LiveClass from, LiveClass to)
		{
			to.CourseId = from.CourseId;
			to.Title = from.Title.Trim();
			to.StartsAt = from.StartsAt;
			to.DurationMinutes = from.DurationMinutes;
			to.MeetingUrl = string.IsNullOrWhiteSpace(from.MeetingUrl) ? null : from.MeetingUrl.Trim();
			to.Notes = from.Notes;
		}
	}
}
