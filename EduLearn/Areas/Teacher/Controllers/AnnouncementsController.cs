using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Areas.Teacher.Controllers
{
	public class AnnouncementsController : TeacherAreaController
	{
		public AnnouncementsController(ApplicationDbContext db) : base(db)
		{
		}

		public async Task<IActionResult> Index()
		{
			var courseIds = await ManageableCourseIdsAsync();
			var list = await Db.Announcements
				.Include(a => a.Course)
				.Where(a => a.CourseId == null || courseIds.Contains(a.CourseId.Value))
				.OrderByDescending(a => a.CreatedAt)
				.ToListAsync();

			ViewBag.Courses = await CourseSelectListAsync();
			return View(list);
		}

		[HttpPost]
		public async Task<IActionResult> Create(int courseId, string title, string message)
		{
			if (!await CanManageCourseAsync(courseId) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
			{
				TempData["Error"] = "Choose one of your courses and enter a title and message.";
				return RedirectToAction(nameof(Index));
			}

			Db.Announcements.Add(new Announcement
			{
				CourseId = courseId,
				Title = title.Trim(),
				Message = message.Trim(),
				CreatedAt = DateTime.Now,
				CreatedByUserId = User.GetUserId(),
				CreatedByName = User.GetDisplayName()
			});
			await Db.SaveChangesAsync();
			TempData["Success"] = "Announcement sent to the course's students.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var a = await Db.Announcements.FindAsync(id);
			// Teachers can only remove announcements on their own courses (site-wide ones belong to the admin)
			if (a?.CourseId == null || !await CanManageCourseAsync(a.CourseId.Value)) return NotFound();

			Db.Announcements.Remove(a);
			await Db.SaveChangesAsync();
			TempData["Success"] = "Announcement deleted.";
			return RedirectToAction(nameof(Index));
		}
	}
}
