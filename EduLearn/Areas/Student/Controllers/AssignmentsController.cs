using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class AssignmentsController : StudentAreaController
	{
		private const long MaxUploadBytes = 110 * 1024 * 1024;
		private readonly IFileService _files;

		public AssignmentsController(ApplicationDbContext db, IAccountService accounts, IFileService files) : base(db, accounts)
		{
			_files = files;
		}

		public async Task<IActionResult> Index(int? courseId = null)
		{
			var courseIds = await AccessibleCourseIdsAsync();
			var assignments = await Db.Assignments
				.Include(a => a.Course)
				.Where(a => courseIds.Contains(a.CourseId) && (courseId == null || a.CourseId == courseId))
				.OrderBy(a => a.DueDate == null).ThenBy(a => a.DueDate)
				.ToListAsync();
			ViewBag.Submissions = await Db.AssignmentSubmissions
				.Where(s => s.StudentId == CurrentStudent.Id)
				.ToDictionaryAsync(s => s.AssignmentId);
			return View(assignments);
		}

		public async Task<IActionResult> Submit(int id)
		{
			var assignment = await LoadAccessibleAsync(id);
			if (assignment == null) return NotFound();

			ViewBag.Submission = await Db.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == CurrentStudent.Id);
			return View(assignment);
		}

		[HttpPost]
		[RequestSizeLimit(MaxUploadBytes)]
		[RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
		public async Task<IActionResult> Submit(int id, string? answer, IFormFile? file)
		{
			var assignment = await LoadAccessibleAsync(id);
			if (assignment == null) return NotFound();

			var submission = await Db.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == CurrentStudent.Id);
			if (submission?.Marks != null)
			{
				TempData["Error"] = "This assignment has already been graded and can't be changed.";
				return RedirectToAction(nameof(Submit), new { id });
			}
			if (assignment.DueDate != null && DateTime.Now > assignment.DueDate)
			{
				TempData["Error"] = "The deadline for this assignment has passed.";
				return RedirectToAction(nameof(Submit), new { id });
			}
			if (string.IsNullOrWhiteSpace(answer) && file == null && submission?.FilePath == null)
			{
				TempData["Error"] = "Write an answer or attach a file.";
				return RedirectToAction(nameof(Submit), new { id });
			}

			string? newFile = null;
			if (file != null)
			{
				var saved = await _files.SaveAsync(file, "submissions", UploadKind.Document);
				if (!saved.Success)
				{
					TempData["Error"] = saved.Error;
					return RedirectToAction(nameof(Submit), new { id });
				}
				newFile = saved.Path;
			}

			if (submission == null)
			{
				submission = new AssignmentSubmission { AssignmentId = id, StudentId = CurrentStudent.Id };
				Db.AssignmentSubmissions.Add(submission);
			}
			submission.Answer = string.IsNullOrWhiteSpace(answer) ? null : answer.Trim();
			submission.SubmittedAt = DateTime.Now;
			if (newFile != null)
			{
				_files.Delete(submission.FilePath);
				submission.FilePath = newFile;
			}
			await Db.SaveChangesAsync();

			TempData["Success"] = "Your work was submitted. You can update it until the deadline or until it's graded.";
			return RedirectToAction(nameof(Submit), new { id });
		}

		private async Task<Assignment?> LoadAccessibleAsync(int id)
		{
			var courseIds = await AccessibleCourseIdsAsync();
			return await Db.Assignments.Include(a => a.Course)
				.FirstOrDefaultAsync(a => a.Id == id && courseIds.Contains(a.CourseId));
		}
	}
}
