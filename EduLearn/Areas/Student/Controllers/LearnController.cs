using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	// The course player
	public class LearnController : StudentAreaController
	{
		private readonly ILearningService _learning;

		public LearnController(ApplicationDbContext db, IAccountService accounts, ILearningService learning) : base(db, accounts)
		{
			_learning = learning;
		}

		// GET /Student/Learn/Index/{courseId}?lessonId=
		public async Task<IActionResult> Index(int id, int? lessonId = null)
		{
			var enrollment = await AccessibleEnrollmentAsync(id);
			if (enrollment == null)
				return RedirectToAction("Index", "Enroll", new { id });

			var course = await Db.Courses
				.Include(c => c.Teacher)
				.Include(c => c.Sections.OrderBy(s => s.SortOrder))
					.ThenInclude(s => s.Lessons.OrderBy(l => l.SortOrder))
				.AsSplitQuery()
				.FirstAsync(c => c.Id == id);

			var lessons = course.Sections.SelectMany(s => s.Lessons).ToList();
			var completed = (await Db.LessonProgresses
				.Where(p => p.EnrollmentId == enrollment.Id)
				.Select(p => p.LessonId)
				.ToListAsync()).ToHashSet();

			// Requested lesson, else the first one not finished yet, else the first lesson
			var current = lessons.FirstOrDefault(l => l.Id == lessonId)
				?? lessons.FirstOrDefault(l => !completed.Contains(l.Id))
				?? lessons.FirstOrDefault();
			var index = current == null ? -1 : lessons.IndexOf(current);

			var vm = new LearnViewModel
			{
				Course = course,
				Enrollment = enrollment,
				Current = current,
				Previous = index > 0 ? lessons[index - 1] : null,
				Next = index >= 0 && index < lessons.Count - 1 ? lessons[index + 1] : null,
				CompletedLessonIds = completed,
				Progress = new CourseProgress(completed.Count(lessons.Select(l => l.Id).Contains), lessons.Count),
				QuizCount = await Db.Quizzes.CountAsync(q => q.CourseId == id && q.IsPublished),
				AssignmentCount = await Db.Assignments.CountAsync(a => a.CourseId == id)
			};
			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> Complete(int lessonId)
		{
			var lesson = await Db.Lessons.Include(l => l.Section).FirstOrDefaultAsync(l => l.Id == lessonId);
			if (lesson == null) return NotFound();

			var courseId = lesson.Section.CourseId;
			var enrollment = await AccessibleEnrollmentAsync(courseId);
			if (enrollment == null) return RedirectToAction("Index", "Enroll", new { id = courseId });

			var wasCompleted = enrollment.Status == EnrollmentStatus.Completed;
			await _learning.MarkLessonCompleteAsync(enrollment, lessonId);

			if (!wasCompleted && enrollment.Status == EnrollmentStatus.Completed)
			{
				TempData["Success"] = "Congratulations! You completed the course. Your certificate is ready.";
				return RedirectToAction("View", "Certificates", new { id = enrollment.Id });
			}

			// Move on to the next lesson in curriculum order
			var ordered = await Db.Lessons
				.Where(l => l.Section.CourseId == courseId)
				.OrderBy(l => l.Section.SortOrder).ThenBy(l => l.SectionId).ThenBy(l => l.SortOrder).ThenBy(l => l.Id)
				.Select(l => l.Id)
				.ToListAsync();
			var i = ordered.IndexOf(lessonId);
			var nextId = i >= 0 && i < ordered.Count - 1 ? ordered[i + 1] : lessonId;
			return RedirectToAction(nameof(Index), new { id = courseId, lessonId = nextId });
		}
	}
}
