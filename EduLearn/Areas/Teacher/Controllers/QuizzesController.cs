using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Areas.Teacher.Controllers
{
	public class QuizzesController : TeacherAreaController
	{
		public QuizzesController(ApplicationDbContext db) : base(db)
		{
		}

		public async Task<IActionResult> Index(int? courseId = null)
		{
			var courseIds = await ManageableCourseIdsAsync();
			var quizzes = await Db.Quizzes
				.Include(q => q.Course)
				.Include(q => q.Questions)
				.Include(q => q.Attempts)
				.Where(q => courseIds.Contains(q.CourseId) && (courseId == null || q.CourseId == courseId))
				.OrderByDescending(q => q.CreatedAt)
				.AsSplitQuery()
				.ToListAsync();

			ViewBag.CourseId = courseId;
			ViewBag.Courses = await CourseSelectListAsync(courseId);
			return View(quizzes);
		}

		public async Task<IActionResult> Create(int? courseId = null)
		{
			ViewBag.Courses = await CourseSelectListAsync(courseId);
			return View("Form", new Quiz { CourseId = courseId ?? 0, TimeLimitMinutes = 15, PassPercentage = 50 });
		}

		[HttpPost]
		public async Task<IActionResult> Create(Quiz model)
		{
			if (!await CanManageCourseAsync(model.CourseId))
				ModelState.AddModelError(nameof(model.CourseId), "Please select one of your courses.");
			if (!ModelState.IsValid)
			{
				ViewBag.Courses = await CourseSelectListAsync(model.CourseId);
				return View("Form", model);
			}

			// New quizzes start unpublished until they have questions
			var quiz = new Quiz { CourseId = model.CourseId, CreatedAt = DateTime.Now, IsPublished = false };
			Copy(model, quiz);
			Db.Quizzes.Add(quiz);
			await Db.SaveChangesAsync();

			TempData["Success"] = "Quiz created. Add the questions, then publish it.";
			return RedirectToAction(nameof(Questions), new { id = quiz.Id });
		}

		public async Task<IActionResult> Edit(int id)
		{
			var quiz = await Db.Quizzes.FindAsync(id);
			if (quiz == null || !await CanManageCourseAsync(quiz.CourseId)) return NotFound();

			ViewBag.Courses = await CourseSelectListAsync(quiz.CourseId);
			return View("Form", quiz);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, Quiz model)
		{
			var quiz = await Db.Quizzes.FindAsync(id);
			if (quiz == null || !await CanManageCourseAsync(quiz.CourseId)) return NotFound();
			if (!await CanManageCourseAsync(model.CourseId))
				ModelState.AddModelError(nameof(model.CourseId), "Please select one of your courses.");
			if (!ModelState.IsValid)
			{
				model.Id = id;
				ViewBag.Courses = await CourseSelectListAsync(model.CourseId);
				return View("Form", model);
			}

			quiz.CourseId = model.CourseId;
			Copy(model, quiz);
			await Db.SaveChangesAsync();
			TempData["Success"] = "Quiz settings saved.";
			return RedirectToAction(nameof(Questions), new { id });
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var quiz = await Db.Quizzes.FindAsync(id);
			if (quiz == null || !await CanManageCourseAsync(quiz.CourseId)) return NotFound();

			// Answers reference questions without cascade, so clear them first
			await Db.QuizAnswers.Where(a => a.Attempt.QuizId == id).ExecuteDeleteAsync();
			Db.Quizzes.Remove(quiz);
			await Db.SaveChangesAsync();
			TempData["Success"] = "Quiz deleted.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> TogglePublish(int id)
		{
			var quiz = await Db.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id);
			if (quiz == null || !await CanManageCourseAsync(quiz.CourseId)) return NotFound();

			if (!quiz.IsPublished && !quiz.Questions.Any())
			{
				TempData["Error"] = "Add at least one question before publishing.";
				return RedirectToAction(nameof(Questions), new { id });
			}

			quiz.IsPublished = !quiz.IsPublished;
			await Db.SaveChangesAsync();
			TempData["Success"] = quiz.IsPublished ? "Quiz published — students can take it now." : "Quiz hidden from students.";
			return RedirectToAction(nameof(Questions), new { id });
		}

		//==================== Questions ====================//

		public async Task<IActionResult> Questions(int id)
		{
			var quiz = await Db.Quizzes
				.Include(q => q.Course)
				.Include(q => q.Questions.OrderBy(x => x.SortOrder)).ThenInclude(x => x.Options.OrderBy(o => o.Id))
				.FirstOrDefaultAsync(q => q.Id == id);
			if (quiz == null || !await CanManageCourseAsync(quiz.CourseId)) return NotFound();

			ViewBag.AttemptCount = await Db.QuizAttempts.CountAsync(a => a.QuizId == id);
			return View(quiz);
		}

		[HttpPost]
		public async Task<IActionResult> AddQuestion(int quizId, string text, int marks, List<string> options, int correctIndex)
		{
			var quiz = await Db.Quizzes.FindAsync(quizId);
			if (quiz == null || !await CanManageCourseAsync(quiz.CourseId)) return NotFound();
			if (await IsLockedAsync(quizId)) return RedirectToAction(nameof(Questions), new { id = quizId });

			var built = BuildOptions(text, marks, options, correctIndex, out var error);
			if (built == null)
			{
				TempData["Error"] = error;
				return RedirectToAction(nameof(Questions), new { id = quizId });
			}

			var next = (await Db.QuizQuestions.Where(q => q.QuizId == quizId).MaxAsync(q => (int?)q.SortOrder) ?? 0) + 1;
			Db.QuizQuestions.Add(new QuizQuestion { QuizId = quizId, Text = text.Trim(), Marks = marks, SortOrder = next, Options = built });
			await Db.SaveChangesAsync();
			TempData["Success"] = "Question added.";
			return RedirectToAction(nameof(Questions), new { id = quizId });
		}

		public async Task<IActionResult> EditQuestion(int id)
		{
			var question = await Db.QuizQuestions.Include(q => q.Quiz).Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == id);
			if (question == null || !await CanManageCourseAsync(question.Quiz.CourseId)) return NotFound();
			if (await IsLockedAsync(question.QuizId)) return RedirectToAction(nameof(Questions), new { id = question.QuizId });

			return View(question);
		}

		[HttpPost]
		public async Task<IActionResult> EditQuestion(int id, string text, int marks, List<string> options, int correctIndex)
		{
			var question = await Db.QuizQuestions.Include(q => q.Quiz).Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == id);
			if (question == null || !await CanManageCourseAsync(question.Quiz.CourseId)) return NotFound();
			if (await IsLockedAsync(question.QuizId)) return RedirectToAction(nameof(Questions), new { id = question.QuizId });

			var built = BuildOptions(text, marks, options, correctIndex, out var error);
			if (built == null)
			{
				TempData["Error"] = error;
				return RedirectToAction(nameof(EditQuestion), new { id });
			}

			question.Text = text.Trim();
			question.Marks = marks;
			Db.QuizOptions.RemoveRange(question.Options);
			question.Options = built;
			await Db.SaveChangesAsync();
			TempData["Success"] = "Question updated.";
			return RedirectToAction(nameof(Questions), new { id = question.QuizId });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteQuestion(int id)
		{
			var question = await Db.QuizQuestions.Include(q => q.Quiz).FirstOrDefaultAsync(q => q.Id == id);
			if (question == null || !await CanManageCourseAsync(question.Quiz.CourseId)) return NotFound();
			if (await IsLockedAsync(question.QuizId)) return RedirectToAction(nameof(Questions), new { id = question.QuizId });

			Db.QuizQuestions.Remove(question);
			await Db.SaveChangesAsync();
			TempData["Success"] = "Question deleted.";
			return RedirectToAction(nameof(Questions), new { id = question.QuizId });
		}

		//==================== Results ====================//

		public async Task<IActionResult> Results(int id)
		{
			var quiz = await Db.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.Id == id);
			if (quiz == null || !await CanManageCourseAsync(quiz.CourseId)) return NotFound();

			ViewBag.Attempts = await Db.QuizAttempts
				.Include(a => a.Student)
				.Where(a => a.QuizId == id && a.SubmittedAt != null)
				.OrderByDescending(a => a.SubmittedAt)
				.ToListAsync();
			return View(quiz);
		}

		[HttpPost]
		public async Task<IActionResult> ResetAttempts(int id)
		{
			var quiz = await Db.Quizzes.FindAsync(id);
			if (quiz == null || !await CanManageCourseAsync(quiz.CourseId)) return NotFound();

			await Db.QuizAnswers.Where(a => a.Attempt.QuizId == id).ExecuteDeleteAsync();
			var removed = await Db.QuizAttempts.Where(a => a.QuizId == id).ExecuteDeleteAsync();
			TempData["Success"] = $"{removed} attempt(s) cleared. You can edit the questions again.";
			return RedirectToAction(nameof(Questions), new { id });
		}

		//==================== Helpers ====================//

		// Once students have answered, changing questions would silently change their past scores
		private async Task<bool> IsLockedAsync(int quizId)
		{
			if (!await Db.QuizAttempts.AnyAsync(a => a.QuizId == quizId)) return false;
			TempData["Error"] = "Students have already attempted this quiz, so its questions are locked. Clear the attempts first if you really need to change them.";
			return true;
		}

		private static List<QuizOption>? BuildOptions(string text, int marks, List<string> options, int correctIndex, out string? error)
		{
			error = null;
			if (string.IsNullOrWhiteSpace(text)) { error = "Question text is required."; return null; }
			if (marks < 1 || marks > 100) { error = "Marks must be between 1 and 100."; return null; }

			var filled = options.Select((o, i) => (Text: o?.Trim(), Index: i)).Where(o => !string.IsNullOrEmpty(o.Text)).ToList();
			if (filled.Count < 2) { error = "Add at least two answer options."; return null; }
			if (!filled.Any(o => o.Index == correctIndex)) { error = "Mark one of the filled options as the correct answer."; return null; }

			return filled.Select(o => new QuizOption { Text = o.Text!, IsCorrect = o.Index == correctIndex }).ToList();
		}

		private static void Copy(Quiz from, Quiz to)
		{
			to.Title = from.Title.Trim();
			to.Description = from.Description;
			to.TimeLimitMinutes = from.TimeLimitMinutes;
			to.PassPercentage = from.PassPercentage;
			to.MaxAttempts = from.MaxAttempts;
		}
	}
}
