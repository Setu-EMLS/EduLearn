using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;
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
			var enrollments = await MyEnrollments()
				.Include(e => e.Course)
				.Include(e => e.Batch).ThenInclude(b => b!.Teacher)
				.Where(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed)
				.OrderBy(e => e.Course.Title)
				.ToListAsync();
			var since = DateTime.Today.AddDays(-14);
			var classes = await Db.LiveClasses.VisibleTo(StudentScope.From(enrollments))
				.Include(l => l.Course)
				.Include(l => l.Batch)
				.Where(l => l.StartsAt >= since)
				.OrderBy(l => l.StartsAt)
				.ToListAsync();
			// Regular class times: the batch's schedule, or the course's when it has no batches
			ViewBag.Enrollments = enrollments;
			return View(classes);
		}
	}
}
