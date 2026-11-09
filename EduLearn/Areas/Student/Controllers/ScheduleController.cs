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
			var scope = StudentScope.From(enrollments);
			var since = DateTime.Today.AddDays(-14);
			var classes = await Db.LiveClasses.VisibleTo(scope)
				.Include(l => l.Course)
				.Include(l => l.Batch)
				.Include(l => l.Topics).ThenInclude(t => t.Lesson).ThenInclude(l => l.Section)
				.Where(l => l.StartsAt >= since)
				.OrderBy(l => l.StartsAt)
				.ToListAsync();
			// Recordings stay available however long ago the class was
			ViewBag.Recordings = await Db.LiveClasses.VisibleTo(scope)
				.Include(l => l.Course)
				.Include(l => l.Batch)
				.Include(l => l.Topics).ThenInclude(t => t.Lesson).ThenInclude(l => l.Section)
				.Where(l => l.RecordingPath != null || l.RecordingUrl != null)
				.OrderByDescending(l => l.StartsAt)
				.ToListAsync();
			// My attendance per class; classes the teacher hasn't marked yet have no entry
			var enrollmentIds = enrollments.Select(e => e.Id).ToList();
			ViewBag.Attendance = await Db.LiveClassAttendances
				.Where(a => enrollmentIds.Contains(a.EnrollmentId))
				.ToDictionaryAsync(a => a.LiveClassId, a => a.Status);
			// Regular class times: the batch's schedule, or the course's when it has no batches
			ViewBag.Enrollments = enrollments;
			return View(classes);
		}

		public async Task<IActionResult> Watch(int id)
		{
			var enrollments = await MyEnrollments()
				.Where(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed)
				.ToListAsync();
			var visible = Db.LiveClasses.VisibleTo(StudentScope.From(enrollments))
				.Where(l => l.RecordingPath != null || l.RecordingUrl != null);

			var live = await visible.Include(l => l.Course).Include(l => l.Batch)
				.Include(l => l.Topics).ThenInclude(t => t.Lesson).ThenInclude(l => l.Section)
				.FirstOrDefaultAsync(l => l.Id == id);
			if (live == null) return NotFound();

			ViewBag.More = await visible
				.Where(l => l.CourseId == live.CourseId && l.Id != id)
				.OrderByDescending(l => l.StartsAt)
				.Take(10)
				.ToListAsync();
			return View(live);
		}
	}
}
