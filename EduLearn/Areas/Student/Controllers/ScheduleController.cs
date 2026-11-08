using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class ScheduleController : StudentAreaController
	{
		public ScheduleController(ApplicationDbContext db, IAccountService accounts) : base(db, accounts)
		{
		}

		public async Task<IActionResult> Index()
		{
			var courseIds = await AccessibleCourseIdsAsync();
			var since = DateTime.Today.AddDays(-14);
			var classes = await Db.LiveClasses
				.Include(l => l.Course)
				.Where(l => courseIds.Contains(l.CourseId) && l.StartsAt >= since)
				.OrderBy(l => l.StartsAt)
				.ToListAsync();
			ViewBag.Courses = await Db.Courses.Where(c => courseIds.Contains(c.Id)).OrderBy(c => c.Title).ToListAsync();
			return View(classes);
		}
	}
}
