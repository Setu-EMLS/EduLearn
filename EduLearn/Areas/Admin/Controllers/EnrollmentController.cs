using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class EnrollmentController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ILearningService _learning;

		public EnrollmentController(ApplicationDbContext context, ILearningService learning)
		{
			_context = context;
			_learning = learning;
		}

		public async Task<IActionResult> Index(EnrollmentStatus? status = null, int? courseId = null, int? batchId = null)
		{
			// Course / batch filters also apply to the counts on the status tabs
			var filtered = _context.Enrollments.AsQueryable();
			if (courseId != null) filtered = filtered.Where(e => e.CourseId == courseId);
			if (batchId != null) filtered = filtered.Where(e => e.BatchId == batchId);

			var query = filtered
				.Include(e => e.Student)
				.Include(e => e.Course)
				.Include(e => e.Batch)
				.AsQueryable();
			if (status != null) query = query.Where(e => e.Status == status);

			var list = await query
				.OrderBy(e => e.Status == EnrollmentStatus.Pending ? 0 : 1)
				.ThenByDescending(e => e.EnrolledAt)
				.ToListAsync();

			ViewBag.Status = status;
			ViewBag.CourseId = courseId;
			ViewBag.BatchId = batchId;
			ViewBag.Courses = new SelectList(await _context.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", courseId);
			ViewBag.Batches = await _context.Batches.OrderByDescending(b => b.StartDate).ThenBy(b => b.Name).ToListAsync();
			ViewBag.Counts = await filtered
				.GroupBy(e => e.Status)
				.Select(g => new { g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.Key, x => x.Count);
			ViewBag.Progress = await _learning.GetProgressAsync(list.Select(e => e.Id));
			return View(list);
		}

		[HttpPost]
		public async Task<IActionResult> Approve(int id, string? returnStatus)
		{
			var enrollment = await _context.Enrollments.Include(e => e.Student).Include(e => e.Course).FirstOrDefaultAsync(e => e.Id == id);
			if (enrollment == null) return NotFound();

			if (enrollment.Status == EnrollmentStatus.Pending || enrollment.Status == EnrollmentStatus.Rejected || enrollment.Status == EnrollmentStatus.Cancelled)
			{
				enrollment.Status = EnrollmentStatus.Active;
				enrollment.ApprovedAt = DateTime.Now;
				await _context.SaveChangesAsync();
				await _learning.RefreshCompletionAsync(enrollment);
				TempData["Success"] = $"{enrollment.Student.Name} now has access to \"{enrollment.Course.Title}\".";
			}
			return RedirectToAction(nameof(Index), new { status = returnStatus });
		}

		[HttpPost]
		public async Task<IActionResult> Reject(int id, string? note, string? returnStatus)
		{
			var enrollment = await _context.Enrollments.Include(e => e.Student).FirstOrDefaultAsync(e => e.Id == id);
			if (enrollment == null) return NotFound();

			enrollment.Status = EnrollmentStatus.Rejected;
			enrollment.AdminNote = string.IsNullOrWhiteSpace(note) ? enrollment.AdminNote : note.Trim();
			await _context.SaveChangesAsync();
			TempData["Success"] = $"Enrollment request from {enrollment.Student.Name} was rejected.";
			return RedirectToAction(nameof(Index), new { status = returnStatus });
		}

		[HttpPost]
		public async Task<IActionResult> Cancel(int id, string? returnStatus)
		{
			var enrollment = await _context.Enrollments.Include(e => e.Student).FirstOrDefaultAsync(e => e.Id == id);
			if (enrollment == null) return NotFound();

			enrollment.Status = EnrollmentStatus.Cancelled;
			await _context.SaveChangesAsync();
			TempData["Success"] = $"Access removed for {enrollment.Student.Name}.";
			return RedirectToAction(nameof(Index), new { status = returnStatus });
		}

		public async Task<IActionResult> Create(int? studentId, int? courseId, int? batchId)
		{
			await LoadLookupsAsync(studentId, courseId, batchId);
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(int studentId, int courseId, int? batchId, decimal? amount, string? paymentMethod, string? transactionId)
		{
			var course = await _context.Courses.Include(c => c.Batches).FirstOrDefaultAsync(c => c.Id == courseId);
			var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
			if (course == null || !studentExists)
			{
				TempData["Error"] = "Please select both a student and a course.";
				await LoadLookupsAsync(studentId, courseId, batchId);
				return View();
			}
			// Courses that run in batches need one; the office may still pick a closed or full batch
			var batch = course.Batches.FirstOrDefault(b => b.Id == batchId);
			if (course.Batches.Any() && batch == null)
			{
				TempData["Error"] = "This course runs in batches. Please choose the student's batch.";
				await LoadLookupsAsync(studentId, courseId, batchId);
				return View();
			}

			var existing = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
			if (existing != null && existing.HasAccess)
			{
				TempData["Error"] = "This student is already enrolled in that course.";
				return RedirectToAction("Details", "Student", new { id = studentId });
			}

			var enrollment = existing ?? new Enrollment { StudentId = studentId, CourseId = courseId };
			enrollment.BatchId = batch?.Id;
			enrollment.Status = EnrollmentStatus.Active;
			enrollment.EnrolledAt = existing?.EnrolledAt ?? DateTime.Now;
			enrollment.ApprovedAt = DateTime.Now;
			enrollment.Amount = amount ?? course.Price ?? 0;
			enrollment.PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Cash (office)" : paymentMethod.Trim();
			enrollment.TransactionId = transactionId?.Trim();
			if (existing == null) _context.Enrollments.Add(enrollment);
			await _context.SaveChangesAsync();
			await _learning.RefreshCompletionAsync(enrollment);

			TempData["Success"] = batch == null ? $"Student enrolled in \"{course.Title}\"." : $"Student enrolled in \"{course.Title}\" ({batch.Name}).";
			return RedirectToAction("Details", "Student", new { id = studentId });
		}

		// Moves a student to another batch of the same course; their progress and work stay with them
		[HttpPost]
		public async Task<IActionResult> ChangeBatch(int id, int batchId, string? returnUrl)
		{
			var enrollment = await _context.Enrollments.Include(e => e.Student).FirstOrDefaultAsync(e => e.Id == id);
			if (enrollment == null) return NotFound();
			var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.CourseId == enrollment.CourseId);
			if (batch == null)
			{
				TempData["Error"] = "Choose a batch of the same course.";
			}
			else if (enrollment.BatchId != batch.Id)
			{
				enrollment.BatchId = batch.Id;
				await _context.SaveChangesAsync();
				TempData["Success"] = $"{enrollment.Student.Name} moved to {batch.Name}.";
			}
			return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
		}

		// Adds several students to a batch at once. They get access straight away (the office has taken the fee).
		[HttpPost]
		public async Task<IActionResult> AddToBatch(int batchId, int[]? studentIds, decimal? amount, string? paymentMethod, string? returnUrl)
		{
			var batch = await _context.Batches.Include(b => b.Course).FirstOrDefaultAsync(b => b.Id == batchId);
			if (batch == null) return NotFound();

			var ids = (studentIds ?? Array.Empty<int>()).Distinct().ToList();
			if (ids.Count == 0)
			{
				TempData["Error"] = "Select at least one student to add.";
				return BackTo(returnUrl, batchId);
			}
			if (amount < 0)
			{
				TempData["Error"] = "The amount can't be negative.";
				return BackTo(returnUrl, batchId);
			}

			var students = await _context.Students.Where(s => ids.Contains(s.Id)).ToListAsync();
			var existing = await _context.Enrollments
				.Where(e => e.CourseId == batch.CourseId && ids.Contains(e.StudentId))
				.ToListAsync();

			var method = string.IsNullOrWhiteSpace(paymentMethod) ? "Cash (office)" : paymentMethod.Trim();
			var added = new List<Enrollment>();
			var skipped = new List<string>();
			foreach (var student in students)
			{
				var enrollment = existing.FirstOrDefault(e => e.StudentId == student.Id);
				if (enrollment != null && enrollment.HasAccess)
				{
					skipped.Add(student.Name);
					continue;
				}

				if (enrollment == null)
				{
					enrollment = new Enrollment { StudentId = student.Id, CourseId = batch.CourseId, EnrolledAt = DateTime.Now };
					_context.Enrollments.Add(enrollment);
				}
				// A pending, rejected or cancelled request is reopened and placed in this batch
				enrollment.BatchId = batch.Id;
				enrollment.Status = EnrollmentStatus.Active;
				enrollment.ApprovedAt = DateTime.Now;
				enrollment.Amount = amount ?? batch.Course.Price ?? 0;
				enrollment.PaymentMethod = method;
				added.Add(enrollment);
			}
			await _context.SaveChangesAsync();
			foreach (var enrollment in added) await _learning.RefreshCompletionAsync(enrollment);

			var message = $"{added.Count} student{(added.Count == 1 ? "" : "s")} added to {batch.Name}.";
			if (skipped.Count > 0)
				message += $" Skipped, already in this course: {string.Join(", ", skipped)} (use \"Move to batch\" to change a student's batch).";
			var seats = (await _learning.SeatsTakenAsync(new[] { batch.Id })).GetValueOrDefault(batch.Id);
			if (batch.Seats > 0 && seats > batch.Seats)
				message += $" Note: the batch now holds {seats} students for {batch.Seats} seats.";
			TempData[added.Count > 0 ? "Success" : "Error"] = added.Count > 0 ? message : "No one was added. " + message;
			return BackTo(returnUrl, batchId);
		}

		//==================== Certificates ====================//

		[HttpPost]
		public Task<IActionResult> IssueCertificate(int id, DateTime? completedOn, string? returnUrl) =>
			IssueCertificatesAsync(new[] { id }, completedOn, returnUrl);

		// For several students at once, e.g. the whole offline batch after the final class
		[HttpPost]
		public Task<IActionResult> IssueCertificates(int[]? ids, DateTime? completedOn, string? returnUrl) =>
			IssueCertificatesAsync(ids ?? Array.Empty<int>(), completedOn, returnUrl);

		private async Task<IActionResult> IssueCertificatesAsync(int[] ids, DateTime? completedOn, string? returnUrl)
		{
			if (ids.Length == 0)
			{
				TempData["Error"] = "Select at least one student.";
				return BackTo(returnUrl);
			}
			if (completedOn != null && completedOn.Value.Date > DateTime.Today)
			{
				TempData["Error"] = "The completion date can't be in the future.";
				return BackTo(returnUrl);
			}

			var enrollments = await _context.Enrollments.Include(e => e.Student)
				.Where(e => ids.Contains(e.Id))
				.ToListAsync();
			var issued = 0;
			var notEligible = new List<string>();
			foreach (var enrollment in enrollments)
			{
				// Only students who have access (Active or already Completed) can be certified
				if (!enrollment.HasAccess)
				{
					notEligible.Add(enrollment.Student.Name);
					continue;
				}
				await _learning.IssueCertificateAsync(enrollment, completedOn);
				issued++;
			}

			var message = issued == 1 ? "Certificate issued." : $"{issued} certificates issued.";
			if (notEligible.Count > 0)
				message += $" Not issued (no active enrollment): {string.Join(", ", notEligible)}.";
			TempData[issued > 0 ? "Success" : "Error"] = message;
			return BackTo(returnUrl);
		}

		[HttpPost]
		public async Task<IActionResult> RevokeCertificate(int id, string? returnUrl)
		{
			var enrollment = await _context.Enrollments.Include(e => e.Student).FirstOrDefaultAsync(e => e.Id == id);
			if (enrollment == null) return NotFound();

			if (enrollment.Status == EnrollmentStatus.Completed)
			{
				await _learning.RevokeCertificateAsync(enrollment);
				TempData["Success"] = $"Certificate of {enrollment.Student.Name} revoked.";
			}
			return BackTo(returnUrl);
		}

		// Printable certificate, e.g. for handing out at the end of an offline batch
		public async Task<IActionResult> Certificate(int id)
		{
			var enrollment = await _context.Enrollments
				.Include(e => e.Student)
				.Include(e => e.Batch).ThenInclude(b => b!.Teacher)
				.Include(e => e.Course).ThenInclude(c => c.Teacher)
				.FirstOrDefaultAsync(e => e.Id == id && e.Status == EnrollmentStatus.Completed);
			if (enrollment == null) return NotFound();
			return View(enrollment);
		}

		private IActionResult BackTo(string? returnUrl, int? batchId = null)
		{
			if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
			return batchId != null
				? RedirectToAction("Details", "Batch", new { id = batchId })
				: RedirectToAction(nameof(Index));
		}

		private async Task LoadLookupsAsync(int? studentId, int? courseId, int? batchId = null)
		{
			ViewBag.BatchId = batchId;
			ViewBag.Batches = await _context.Batches.OrderByDescending(b => b.StartDate).ThenBy(b => b.Name).ToListAsync();
			var students = await _context.Students.OrderBy(s => s.Name)
				.Select(s => new { s.Id, Label = s.Name + " (" + s.StudentID + ")" }).ToListAsync();
			ViewBag.Students = new SelectList(students, "Id", "Label", studentId);
			var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
			ViewBag.Courses = new SelectList(courses, "Id", "Title", courseId);
			ViewBag.CoursePrices = courses.ToDictionary(c => c.Id, c => c.Price ?? 0);
		}
	}
}
