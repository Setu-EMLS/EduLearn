using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class GradesController : StudentAreaController
	{
		private readonly ILearningService _learning;

		public GradesController(ApplicationDbContext db, IAccountService accounts, ILearningService learning) : base(db, accounts)
		{
			_learning = learning;
		}

		public async Task<IActionResult> Index()
		{
			var enrollments = await MyEnrollments()
				.Include(e => e.Course)
				.Where(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed)
				.OrderBy(e => e.Course.Title)
				.ToListAsync();
			var courseIds = enrollments.Select(e => e.CourseId).ToList();

			ViewBag.Progress = await _learning.GetProgressAsync(enrollments.Select(e => e.Id));
			ViewBag.Assignments = await Db.Assignments
				.Where(a => courseIds.Contains(a.CourseId))
				.OrderBy(a => a.DueDate)
				.ToListAsync();
			ViewBag.Submissions = await Db.AssignmentSubmissions
				.Where(s => s.StudentId == CurrentStudent.Id)
				.ToDictionaryAsync(s => s.AssignmentId);
			ViewBag.Quizzes = await Db.Quizzes
				.Where(q => q.IsPublished && courseIds.Contains(q.CourseId))
				.OrderBy(q => q.CreatedAt)
				.ToListAsync();
			ViewBag.BestAttempts = (await Db.QuizAttempts
				.Where(a => a.StudentId == CurrentStudent.Id && a.SubmittedAt != null)
				.ToListAsync())
				.GroupBy(a => a.QuizId)
				.ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.Percent).First());
			return View(enrollments);
		}
	}
}
