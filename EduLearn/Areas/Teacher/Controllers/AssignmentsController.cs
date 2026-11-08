using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Teacher.Controllers
{
	public class AssignmentsController : TeacherAreaController
	{
		private readonly IFileService _files;

		public AssignmentsController(ApplicationDbContext db, IFileService files) : base(db)
		{
			_files = files;
		}

		public async Task<IActionResult> Index(int? courseId = null)
		{
			var courseIds = await ManageableCourseIdsAsync();
			var list = await Db.Assignments
				.Include(a => a.Course)
				.Include(a => a.Submissions)
				.Where(a => courseIds.Contains(a.CourseId) && (courseId == null || a.CourseId == courseId))
				.OrderByDescending(a => a.CreatedAt)
				.ToListAsync();

			ViewBag.CourseId = courseId;
			ViewBag.Courses = await CourseSelectListAsync(courseId);
			return View(list);
		}

		public async Task<IActionResult> Create(int? courseId = null)
		{
			ViewBag.Courses = await CourseSelectListAsync(courseId);
			return View("Form", new Assignment { CourseId = courseId ?? 0, DueDate = DateTime.Today.AddDays(7).AddHours(23).AddMinutes(59) });
		}

		[HttpPost]
		public async Task<IActionResult> Create(Assignment model)
		{
			if (!await CanManageCourseAsync(model.CourseId))
				ModelState.AddModelError(nameof(model.CourseId), "Please select one of your courses.");

			string? attachment = null;
			if (ModelState.IsValid && model.Attachment != null)
			{
				var saved = await _files.SaveAsync(model.Attachment, "assignments", UploadKind.Document);
				if (saved.Success) attachment = saved.Path;
				else ModelState.AddModelError(nameof(model.Attachment), saved.Error!);
			}

			if (!ModelState.IsValid)
			{
				ViewBag.Courses = await CourseSelectListAsync(model.CourseId);
				return View("Form", model);
			}

			var assignment = new Assignment { CourseId = model.CourseId, CreatedAt = DateTime.Now, AttachmentPath = attachment };
			Copy(model, assignment);
			Db.Assignments.Add(assignment);
			await Db.SaveChangesAsync();

			TempData["Success"] = "Assignment published to students.";
			return RedirectToAction(nameof(Index), new { courseId = assignment.CourseId });
		}

		public async Task<IActionResult> Edit(int id)
		{
			var assignment = await Db.Assignments.FindAsync(id);
			if (assignment == null || !await CanManageCourseAsync(assignment.CourseId)) return NotFound();

			ViewBag.Courses = await CourseSelectListAsync(assignment.CourseId);
			return View("Form", assignment);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, Assignment model, bool removeAttachment = false)
		{
			var assignment = await Db.Assignments.FindAsync(id);
			if (assignment == null || !await CanManageCourseAsync(assignment.CourseId)) return NotFound();
			if (!await CanManageCourseAsync(model.CourseId))
				ModelState.AddModelError(nameof(model.CourseId), "Please select one of your courses.");

			string? attachment = null;
			if (ModelState.IsValid && model.Attachment != null)
			{
				var saved = await _files.SaveAsync(model.Attachment, "assignments", UploadKind.Document);
				if (saved.Success) attachment = saved.Path;
				else ModelState.AddModelError(nameof(model.Attachment), saved.Error!);
			}

			if (!ModelState.IsValid)
			{
				model.Id = id;
				model.AttachmentPath = assignment.AttachmentPath;
				ViewBag.Courses = await CourseSelectListAsync(model.CourseId);
				return View("Form", model);
			}

			assignment.CourseId = model.CourseId;
			Copy(model, assignment);
			if (attachment != null || removeAttachment)
			{
				_files.Delete(assignment.AttachmentPath);
				assignment.AttachmentPath = attachment;
			}
			await Db.SaveChangesAsync();

			TempData["Success"] = "Assignment updated.";
			return RedirectToAction(nameof(Index), new { courseId = assignment.CourseId });
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var assignment = await Db.Assignments.Include(a => a.Submissions).FirstOrDefaultAsync(a => a.Id == id);
			if (assignment == null || !await CanManageCourseAsync(assignment.CourseId)) return NotFound();

			var files = assignment.Submissions.Select(s => s.FilePath).Append(assignment.AttachmentPath).ToList();
			Db.Assignments.Remove(assignment);
			await Db.SaveChangesAsync();
			foreach (var f in files) _files.Delete(f);

			TempData["Success"] = "Assignment deleted.";
			return RedirectToAction(nameof(Index));
		}

		// Every student with access, with their submission if any
		public async Task<IActionResult> Submissions(int id)
		{
			var assignment = await Db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);
			if (assignment == null || !await CanManageCourseAsync(assignment.CourseId)) return NotFound();

			var students = await Db.Enrollments
				.Where(e => e.CourseId == assignment.CourseId && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed))
				.Select(e => e.Student)
				.OrderBy(s => s.Name)
				.ToListAsync();
			var submissions = await Db.AssignmentSubmissions
				.Where(s => s.AssignmentId == id)
				.ToDictionaryAsync(s => s.StudentId);

			ViewBag.Students = students;
			ViewBag.Submissions = submissions;
			return View(assignment);
		}

		[HttpPost]
		public async Task<IActionResult> Grade(int submissionId, decimal? marks, string? feedback)
		{
			var submission = await Db.AssignmentSubmissions.Include(s => s.Assignment).FirstOrDefaultAsync(s => s.Id == submissionId);
			if (submission == null || !await CanManageCourseAsync(submission.Assignment.CourseId)) return NotFound();

			if (marks == null || marks < 0 || marks > submission.Assignment.MaxMarks)
			{
				TempData["Error"] = $"Marks must be between 0 and {submission.Assignment.MaxMarks}.";
				return RedirectToAction(nameof(Submissions), new { id = submission.AssignmentId });
			}

			submission.Marks = marks;
			submission.Feedback = string.IsNullOrWhiteSpace(feedback) ? null : feedback.Trim();
			submission.GradedAt = DateTime.Now;
			await Db.SaveChangesAsync();

			TempData["Success"] = "Grade saved.";
			return RedirectToAction(nameof(Submissions), new { id = submission.AssignmentId });
		}

		private static void Copy(Assignment from, Assignment to)
		{
			to.Title = from.Title.Trim();
			to.Instructions = from.Instructions;
			to.DueDate = from.DueDate;
			to.MaxMarks = from.MaxMarks;
		}
	}
}
