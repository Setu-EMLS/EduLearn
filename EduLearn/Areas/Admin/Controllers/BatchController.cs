using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class BatchController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ILearningService _learning;
		private readonly IFileService _files;

		public BatchController(ApplicationDbContext context, ILearningService learning, IFileService files)
		{
			_context = context;
			_learning = learning;
			_files = files;
		}

		public async Task<IActionResult> Index(int? courseId = null, BatchPhase? phase = null)
		{
			var batches = await _context.Batches
				.Include(b => b.Course).ThenInclude(c => c.Teacher)
				.Include(b => b.Teacher)
				.Include(b => b.Enrollments)
				.Where(b => courseId == null || b.CourseId == courseId)
				.OrderByDescending(b => b.StartDate).ThenBy(b => b.Name)
				.ToListAsync();

			// Upcoming / running / finished depends on today's date, so it's worked out here
			ViewBag.PhaseCounts = batches.GroupBy(b => b.Phase).ToDictionary(g => g.Key, g => g.Count());
			ViewBag.Total = batches.Count;
			if (phase != null) batches = batches.Where(b => b.Phase == phase).ToList();

			ViewBag.CourseId = courseId;
			ViewBag.Phase = phase;
			ViewBag.Courses = new SelectList(await _context.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", courseId);
			return View(batches);
		}

		public async Task<IActionResult> Details(int id)
		{
			var batch = await _context.Batches
				.Include(b => b.Course).ThenInclude(c => c.Teacher)
				.Include(b => b.Teacher)
				.Include(b => b.Enrollments).ThenInclude(e => e.Student)
				.AsSplitQuery()
				.FirstOrDefaultAsync(b => b.Id == id);
			if (batch == null) return NotFound();

			ViewBag.Progress = await _learning.GetProgressAsync(batch.Enrollments.Select(e => e.Id));

			// Students who don't have access to this course yet (in any batch), for the "Add students" picker
			var withAccess = await _context.Enrollments
				.Where(e => e.CourseId == batch.CourseId && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed))
				.Select(e => e.StudentId)
				.ToListAsync();
			ViewBag.AvailableStudents = await _context.Students
				.Where(s => !withAccess.Contains(s.Id))
				.OrderBy(s => s.Name)
				.Select(s => new StudentPick(s.Id, s.Name, s.StudentID, s.Contact))
				.ToListAsync();
			ViewBag.OtherBatches = await _context.Batches
				.Where(b => b.CourseId == batch.CourseId && b.Id != id)
				.OrderByDescending(b => b.StartDate)
				.ToListAsync();
			ViewBag.LiveClasses = await _context.LiveClasses
				.Where(l => l.CourseId == batch.CourseId && (l.BatchId == null || l.BatchId == id) && l.StartsAt >= DateTime.Today)
				.OrderBy(l => l.StartsAt)
				.Take(5)
				.ToListAsync();
			return View(batch);
		}

		public async Task<IActionResult> Create(int? courseId = null)
		{
			if (!await _context.Courses.AnyAsync())
			{
				TempData["Error"] = "Create a course first.";
				return RedirectToAction("Index", "Course");
			}

			var model = new Batch { CourseId = courseId ?? 0, IsAdmissionOpen = true };
			var course = courseId == null ? null : await _context.Courses.FindAsync(courseId);
			if (course != null)
			{
				// Suggest the next name and the course's usual schedule and seats
				var count = await _context.Batches.CountAsync(b => b.CourseId == course.Id);
				model.Name = $"Batch {count + 1:00}";
				model.Schedule = course.Schedule;
				model.Seats = course.Seats > 0 ? course.Seats : null;
			}
			await LoadLookupsAsync(model.CourseId, model.TeacherId);
			return View("Form", model);
		}

		[HttpPost]
		public async Task<IActionResult> Create(Batch model)
		{
			await ValidateAsync(model, null);
			if (!ModelState.IsValid)
			{
				await LoadLookupsAsync(model.CourseId, model.TeacherId);
				return View("Form", model);
			}

			var batch = new Batch { CourseId = model.CourseId, CreatedAt = DateTime.Now };
			Copy(model, batch);
			_context.Batches.Add(batch);
			await _context.SaveChangesAsync();

			TempData["Success"] = $"{batch.Name} created.{(batch.IsAdmissionOpen ? " Students can now choose it when they enroll." : "")}";
			return RedirectToAction(nameof(Index), new { courseId = batch.CourseId });
		}

		public async Task<IActionResult> Edit(int id)
		{
			var batch = await _context.Batches.Include(b => b.Course).FirstOrDefaultAsync(b => b.Id == id);
			if (batch == null) return NotFound();
			await LoadLookupsAsync(batch.CourseId, batch.TeacherId);
			return View("Form", batch);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, Batch model)
		{
			var batch = await _context.Batches.Include(b => b.Course).FirstOrDefaultAsync(b => b.Id == id);
			if (batch == null) return NotFound();

			// A batch stays with its course: its students, classes and assignments belong to it
			model.CourseId = batch.CourseId;
			await ValidateAsync(model, id);
			if (!ModelState.IsValid)
			{
				model.Id = id;
				model.Course = batch.Course;
				await LoadLookupsAsync(model.CourseId, model.TeacherId);
				return View("Form", model);
			}

			Copy(model, batch);
			await _context.SaveChangesAsync();
			TempData["Success"] = $"{batch.Name} updated.";
			return RedirectToAction(nameof(Index), new { courseId = batch.CourseId });
		}

		[HttpPost]
		public async Task<IActionResult> ToggleAdmission(int id, string? returnUrl)
		{
			var batch = await _context.Batches.FindAsync(id);
			if (batch == null) return NotFound();

			batch.IsAdmissionOpen = !batch.IsAdmissionOpen;
			await _context.SaveChangesAsync();
			TempData["Success"] = batch.IsAdmissionOpen
				? $"Admission for {batch.Name} is open."
				: $"Admission for {batch.Name} is closed. Its students keep their access.";
			return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index), new { courseId = batch.CourseId });
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var batch = await _context.Batches.Include(b => b.Enrollments).FirstOrDefaultAsync(b => b.Id == id);
			if (batch == null) return NotFound();

			if (batch.Enrollments.Any())
			{
				TempData["Error"] = $"{batch.Name} has {batch.Enrollments.Count} enrollment(s) and cannot be deleted. Close its admission instead, or move the students to another batch first.";
				return RedirectToAction(nameof(Index), new { courseId = batch.CourseId });
			}

			// Live classes, announcements and assignments made only for this batch go with it
			var assignments = await _context.Assignments.Include(a => a.Submissions).Where(a => a.BatchId == id).ToListAsync();
			var files = assignments.SelectMany(a => a.Submissions.Select(s => s.FilePath).Append(a.AttachmentPath)).ToList();
			files.AddRange(await _context.LiveClasses.Where(l => l.BatchId == id && l.RecordingPath != null).Select(l => l.RecordingPath).ToListAsync());
			_context.Assignments.RemoveRange(assignments);
			_context.LiveClassTopics.RemoveRange(_context.LiveClassTopics.Where(t => t.LiveClass.BatchId == id));
			_context.LiveClasses.RemoveRange(_context.LiveClasses.Where(l => l.BatchId == id));
			_context.Announcements.RemoveRange(_context.Announcements.Where(a => a.BatchId == id));
			_context.Batches.Remove(batch);
			await _context.SaveChangesAsync();
			foreach (var f in files) _files.Delete(f);

			TempData["Success"] = $"{batch.Name} deleted.";
			return RedirectToAction(nameof(Index), new { courseId = batch.CourseId });
		}

		private async Task ValidateAsync(Batch model, int? id)
		{
			if (!await _context.Courses.AnyAsync(c => c.Id == model.CourseId))
				ModelState.AddModelError(nameof(model.CourseId), "Please select a course.");
			if (model.TeacherId != null && !await _context.Teachers.AnyAsync(t => t.Id == model.TeacherId))
				ModelState.AddModelError(nameof(model.TeacherId), "Please select a valid trainer.");
			if (model.EndDate != null && model.EndDate.Value.Date < model.StartDate.Date)
				ModelState.AddModelError(nameof(model.EndDate), "The end date can't be before the start date.");

			var name = model.Name?.Trim();
			if (!string.IsNullOrEmpty(name) && await _context.Batches.AnyAsync(b => b.CourseId == model.CourseId && b.Name == name && b.Id != id))
				ModelState.AddModelError(nameof(model.Name), "This course already has a batch with that name.");
		}

		private static void Copy(Batch from, Batch to)
		{
			to.Name = from.Name.Trim();
			to.TeacherId = from.TeacherId;
			to.StartDate = from.StartDate.Date;
			to.EndDate = from.EndDate?.Date;
			to.Schedule = string.IsNullOrWhiteSpace(from.Schedule) ? null : from.Schedule.Trim();
			to.Seats = from.Seats > 0 ? from.Seats : null;
			to.IsAdmissionOpen = from.IsAdmissionOpen;
		}

		private async Task LoadLookupsAsync(int? courseId, int? teacherId)
		{
			ViewBag.Courses = new SelectList(await _context.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", courseId);
			ViewBag.Teachers = new SelectList(await _context.Teachers.OrderBy(t => t.Name).ToListAsync(), "Id", "Name", teacherId);
		}
	}
}
