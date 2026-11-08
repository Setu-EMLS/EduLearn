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

			var batches = await _context.Batches
				.Select(b => new BatchStatRow
				{
					BatchId = b.Id,
					Name = b.Name,
					CourseTitle = b.Course.Title,
					TrainerName = b.Teacher != null ? b.Teacher.Name : b.Course.Teacher != null ? b.Course.Teacher.Name : null,
					StartDate = b.StartDate,
					EndDate = b.EndDate,
					Seats = b.Seats,
					Enrolled = b.Enrollments.Count(e => paid.Contains(e.Status)),
					Completed = b.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
					Pending = b.Enrollments.Count(e => e.Status == EnrollmentStatus.Pending),
					Revenue = b.Enrollments.Where(e => paid.Contains(e.Status)).Sum(e => (decimal?)e.Amount) ?? 0
				})
				.OrderByDescending(b => b.StartDate).ThenBy(b => b.Name)
				.ToListAsync();

			// Average lesson progress per course (and per batch) across learners who have access
			var accessIds = await _context.Enrollments
				.Where(e => paid.Contains(e.Status))
				.Select(e => new { e.Id, e.CourseId, e.BatchId })
				.ToListAsync();
			var progress = await _learning.GetProgressAsync(accessIds.Select(x => x.Id));
			foreach (var row in courses)
			{
				var values = accessIds.Where(x => x.CourseId == row.CourseId).Select(x => progress[x.Id].Percent).ToList();
				row.AverageProgress = values.Count == 0 ? 0 : (int)Math.Round(values.Average());
			}
			foreach (var row in batches)
			{
				var values = accessIds.Where(x => x.BatchId == row.BatchId).Select(x => progress[x.Id].Percent).ToList();
				row.AverageProgress = values.Count == 0 ? 0 : (int)Math.Round(values.Average());
			}
			ViewBag.Batches = batches;

			var toExclusive = to?.Date.AddDays(1);
			var payments = await _context.Enrollments
				.Include(e => e.Student)
				.Include(e => e.Course)
				.Include(e => e.Batch)
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
