using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class ReportController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ILearningService _learning;

		public ReportController(ApplicationDbContext context, ILearningService learning)
		{
			_context = context;
			_learning = learning;
		}

		public async Task<IActionResult> Index(DateTime? from = null, DateTime? to = null)
		{
			var paid = new[] { EnrollmentStatus.Active, EnrollmentStatus.Completed };

			var courses = await _context.Courses
				.Select(c => new CourseStatRow
				{
					CourseId = c.Id,
					Title = c.Title,
					TeacherName = c.Teacher != null ? c.Teacher.Name : null,
					Price = c.Price,
					IsPublished = c.IsPublished,
					Enrolled = c.Enrollments.Count(e => paid.Contains(e.Status)),
					Active = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
					Completed = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
					Pending = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Pending),
					Revenue = c.Enrollments.Where(e => paid.Contains(e.Status)).Sum(e => (decimal?)e.Amount) ?? 0
				})
				.OrderBy(c => c.Title)
				.ToListAsync();

			// Average lesson progress per course across learners who have access
			var accessIds = await _context.Enrollments
				.Where(e => paid.Contains(e.Status))
				.Select(e => new { e.Id, e.CourseId })
				.ToListAsync();
			var progress = await _learning.GetProgressAsync(accessIds.Select(x => x.Id));
			foreach (var row in courses)
			{
				var values = accessIds.Where(x => x.CourseId == row.CourseId).Select(x => progress[x.Id].Percent).ToList();
				row.AverageProgress = values.Count == 0 ? 0 : (int)Math.Round(values.Average());
			}

			var toExclusive = to?.Date.AddDays(1);
			var payments = await _context.Enrollments
				.Include(e => e.Student)
				.Include(e => e.Course)
				.Where(e => paid.Contains(e.Status) && e.Amount > 0)
				.Where(e => from == null || (e.ApprovedAt ?? e.EnrolledAt) >= from.Value.Date)
				.Where(e => toExclusive == null || (e.ApprovedAt ?? e.EnrolledAt) < toExclusive)
				.OrderByDescending(e => e.ApprovedAt ?? e.EnrolledAt)
				.Take(500)
				.ToListAsync();

			ViewBag.Payments = payments;
			ViewBag.From = from;
			ViewBag.To = to;
			return View(courses);
		}
	}
}
