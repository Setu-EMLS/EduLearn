using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Teacher.Controllers
{
	public class CoursesController : TeacherAreaController
	{
		private readonly ILearningService _learning;

		public CoursesController(ApplicationDbContext db, ILearningService learning) : base(db)
		{
			_learning = learning;
		}

		public async Task<IActionResult> Index()
		{
			var courses = await (await ManageableCoursesAsync())
				.Include(c => c.Coursecategory)
				.Include(c => c.Enrollments)
				.Include(c => c.Sections).ThenInclude(s => s.Lessons)
				.OrderBy(c => c.Title)
				.AsSplitQuery()
				.ToListAsync();
			return View(courses);
		}

		// Enrolled students with their lesson progress
		public async Task<IActionResult> Students(int id)
		{
			if (!await CanManageCourseAsync(id)) return NotFound();

			var course = await Db.Courses
				.Include(c => c.Enrollments).ThenInclude(e => e.Student)
				.FirstAsync(c => c.Id == id);
			var enrollments = course.Enrollments.Where(e => e.HasAccess).OrderBy(e => e.Student.Name).ToList();

			ViewBag.Progress = await _learning.GetProgressAsync(enrollments.Select(e => e.Id));
			ViewBag.Enrollments = enrollments;

			var studentIds = enrollments.Select(e => e.StudentId).ToList();
			ViewBag.AssignmentCount = await Db.Assignments.CountAsync(a => a.CourseId == id);
			ViewBag.Submitted = await Db.AssignmentSubmissions
				.Where(s => s.Assignment.CourseId == id && studentIds.Contains(s.StudentId))
				.GroupBy(s => s.StudentId)
				.Select(g => new { g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.Key, x => x.Count);
			ViewBag.BestQuiz = await Db.QuizAttempts
				.Where(a => a.Quiz.CourseId == id && a.SubmittedAt != null && a.TotalMarks > 0 && studentIds.Contains(a.StudentId))
				.GroupBy(a => a.StudentId)
				.Select(g => new { g.Key, Avg = g.Average(a => a.Score * 100 / a.TotalMarks) })
				.ToDictionaryAsync(x => x.Key, x => (int)Math.Round(x.Avg));
			return View(course);
		}
	}
}
