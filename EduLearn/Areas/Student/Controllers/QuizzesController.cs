using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class QuizzesController : StudentAreaController
	{
		// Extra time accepted after the limit, to cover slow connections / auto-submit
		private static readonly TimeSpan Grace = TimeSpan.FromSeconds(60);

		public QuizzesController(ApplicationDbContext db, IAccountService accounts) : base(db, accounts)
		{
		}

		public async Task<IActionResult> Index(int? courseId = null)
		{
			var courseIds = await AccessibleCourseIdsAsync();
			var quizzes = await Db.Quizzes
				.Include(q => q.Course)
				.Include(q => q.Questions)
				.Where(q => q.IsPublished && courseIds.Contains(q.CourseId) && (courseId == null || q.CourseId == courseId))
				.OrderBy(q => q.Course.Title).ThenBy(q => q.CreatedAt)
				.ToListAsync();
			var quizIds = quizzes.Select(q => q.Id).ToList();
			ViewBag.Attempts = (await Db.QuizAttempts
				.Where(a => a.StudentId == CurrentStudent.Id && quizIds.Contains(a.QuizId))
				.ToListAsync())
				.GroupBy(a => a.QuizId)
				.ToDictionary(g => g.Key, g => g.ToList());
			return View(quizzes);
		}

		[HttpPost]
		public async Task<IActionResult> Start(int id)
		{
			var quiz = await LoadAccessibleQuizAsync(id);
			if (quiz == null) return NotFound();
			if (!quiz.Questions.Any())
			{
				TempData["Error"] = "This quiz has no questions yet.";
				return RedirectToAction(nameof(Index));
			}

			var attempts = await Db.QuizAttempts.Where(a => a.QuizId == id && a.StudentId == CurrentStudent.Id).ToListAsync();

			// Resume an unfinished attempt that still has time left
			var open = attempts.FirstOrDefault(a => a.SubmittedAt == null);
			if (open != null && (quiz.TimeLimitMinutes == null || DateTime.Now < open.StartedAt.AddMinutes(quiz.TimeLimitMinutes.Value) + Grace))
				return RedirectToAction(nameof(Take), new { id = open.Id });
			if (open != null)
			{
				// Time ran out without a submission: close it with whatever was saved (nothing)
				await GradeAsync(open, quiz, new Dictionary<int, int>());
			}

			var used = attempts.Count;
			if (quiz.MaxAttempts != null && used >= quiz.MaxAttempts)
			{
				TempData["Error"] = $"You have used all {quiz.MaxAttempts} attempt(s) for this quiz.";
				return RedirectToAction(nameof(Index));
			}

			var attempt = new QuizAttempt
			{
				QuizId = id,
				StudentId = CurrentStudent.Id,
				StartedAt = DateTime.Now,
				TotalMarks = quiz.Questions.Sum(q => q.Marks)
			};
			Db.QuizAttempts.Add(attempt);
			await Db.SaveChangesAsync();
			return RedirectToAction(nameof(Take), new { id = attempt.Id });
		}

		// GET /Student/Quizzes/Take/{attemptId}
		public async Task<IActionResult> Take(int id)
		{
			var attempt = await Db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == id && a.StudentId == CurrentStudent.Id);
			if (attempt == null) return NotFound();
			if (attempt.SubmittedAt != null) return RedirectToAction(nameof(Result), new { id });

			var quiz = await LoadAccessibleQuizAsync(attempt.QuizId);
			if (quiz == null) return NotFound();

			ViewBag.Attempt = attempt;
			ViewBag.SecondsLeft = quiz.TimeLimitMinutes == null
				? (int?)null
				: Math.Max(0, (int)(attempt.StartedAt.AddMinutes(quiz.TimeLimitMinutes.Value) - DateTime.Now).TotalSeconds);
			return View(quiz);
		}

		[HttpPost]
		public async Task<IActionResult> Submit(int id, IFormCollection form)
		{
			var attempt = await Db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == id && a.StudentId == CurrentStudent.Id);
			if (attempt == null) return NotFound();
			if (attempt.SubmittedAt != null) return RedirectToAction(nameof(Result), new { id });

			var quiz = await LoadAccessibleQuizAsync(attempt.QuizId);
			if (quiz == null) return NotFound();

			var answers = new Dictionary<int, int>();
			var late = quiz.TimeLimitMinutes != null && DateTime.Now > attempt.StartedAt.AddMinutes(quiz.TimeLimitMinutes.Value) + Grace;
			if (!late)
			{
				foreach (var q in quiz.Questions)
				{
					if (int.TryParse(form[$"q_{q.Id}"], out var optionId) && q.Options.Any(o => o.Id == optionId))
						answers[q.Id] = optionId;
				}
			}
			else
			{
				TempData["Error"] = "Time was up before the quiz was submitted, so the answers could not be counted.";
			}

			await GradeAsync(attempt, quiz, answers);
			return RedirectToAction(nameof(Result), new { id });
		}

		public async Task<IActionResult> Result(int id)
		{
			var attempt = await Db.QuizAttempts
				.Include(a => a.Answers)
				.FirstOrDefaultAsync(a => a.Id == id && a.StudentId == CurrentStudent.Id);
			if (attempt == null) return NotFound();
			if (attempt.SubmittedAt == null) return RedirectToAction(nameof(Take), new { id });

			var quiz = await Db.Quizzes
				.Include(q => q.Course)
				.Include(q => q.Questions.OrderBy(x => x.SortOrder)).ThenInclude(x => x.Options.OrderBy(o => o.Id))
				.FirstAsync(q => q.Id == attempt.QuizId);

			var used = await Db.QuizAttempts.CountAsync(a => a.QuizId == quiz.Id && a.StudentId == CurrentStudent.Id);
			ViewBag.Attempt = attempt;
			ViewBag.CanRetry = quiz.IsPublished && (quiz.MaxAttempts == null || used < quiz.MaxAttempts);
			// Reveal the right answers only once no attempts remain, so retakes stay meaningful
			ViewBag.RevealCorrect = quiz.MaxAttempts != null && used >= quiz.MaxAttempts;
			return View(quiz);
		}

		private async Task GradeAsync(QuizAttempt attempt, Quiz quiz, Dictionary<int, int> answers)
		{
			decimal score = 0;
			foreach (var q in quiz.Questions)
			{
				answers.TryGetValue(q.Id, out var optionId);
				var correct = optionId != 0 && q.Options.Any(o => o.Id == optionId && o.IsCorrect);
				if (correct) score += q.Marks;
				Db.QuizAnswers.Add(new QuizAnswer
				{
					AttemptId = attempt.Id,
					QuestionId = q.Id,
					SelectedOptionId = optionId == 0 ? null : optionId,
					IsCorrect = correct
				});
			}

			attempt.TotalMarks = quiz.Questions.Sum(q => q.Marks);
			attempt.Score = score;
			attempt.SubmittedAt = DateTime.Now;
			attempt.Passed = attempt.TotalMarks > 0 && score * 100 / attempt.TotalMarks >= quiz.PassPercentage;
			await Db.SaveChangesAsync();
		}

		private async Task<Quiz?> LoadAccessibleQuizAsync(int quizId)
		{
			var courseIds = await AccessibleCourseIdsAsync();
			return await Db.Quizzes
				.Include(q => q.Course)
				.Include(q => q.Questions.OrderBy(x => x.SortOrder)).ThenInclude(x => x.Options.OrderBy(o => o.Id))
				.FirstOrDefaultAsync(q => q.Id == quizId && q.IsPublished && courseIds.Contains(q.CourseId));
		}
	}
}
