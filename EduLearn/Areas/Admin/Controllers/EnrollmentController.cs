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

		public async Task<IActionResult> Index(EnrollmentStatus? status = null, int? courseId = null)
		{
			var query = _context.Enrollments
				.Include(e => e.Student)
				.Include(e => e.Course)
				.AsQueryable();
			if (status != null) query = query.Where(e => e.Status == status);
			if (courseId != null) query = query.Where(e => e.CourseId == courseId);

			var list = await query
				.OrderBy(e => e.Status == EnrollmentStatus.Pending ? 0 : 1)
				.ThenByDescending(e => e.EnrolledAt)
				.ToListAsync();

			ViewBag.Status = status;
			ViewBag.CourseId = courseId;
			ViewBag.Courses = new SelectList(await _context.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", courseId);
			ViewBag.Counts = await _context.Enrollments
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

		public async Task<IActionResult> Create(int? studentId, int? courseId)
		{
			await LoadLookupsAsync(studentId, courseId);
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(int studentId, int courseId, decimal? amount, string? paymentMethod, string? transactionId)
		{
			var course = await _context.Courses.FindAsync(courseId);
			var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
			if (course == null || !studentExists)
			{
				TempData["Error"] = "Please select both a student and a course.";
				await LoadLookupsAsync(studentId, courseId);
				return View();
			}

			var existing = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
			if (existing != null && existing.HasAccess)
			{
				TempData["Error"] = "This student is already enrolled in that course.";
				return RedirectToAction("Details", "Student", new { id = studentId });
			}

			var enrollment = existing ?? new Enrollment { StudentId = studentId, CourseId = courseId };
			enrollment.Status = EnrollmentStatus.Active;
			enrollment.EnrolledAt = existing?.EnrolledAt ?? DateTime.Now;
			enrollment.ApprovedAt = DateTime.Now;
			enrollment.Amount = amount ?? course.Price ?? 0;
			enrollment.PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Cash (office)" : paymentMethod.Trim();
			enrollment.TransactionId = transactionId?.Trim();
			if (existing == null) _context.Enrollments.Add(enrollment);
			await _context.SaveChangesAsync();
			await _learning.RefreshCompletionAsync(enrollment);

			TempData["Success"] = $"Student enrolled in \"{course.Title}\".";
			return RedirectToAction("Details", "Student", new { id = studentId });
		}

		private async Task LoadLookupsAsync(int? studentId, int? courseId)
		{
			var students = await _context.Students.OrderBy(s => s.Name)
				.Select(s => new { s.Id, Label = s.Name + " (" + s.StudentID + ")" }).ToListAsync();
			ViewBag.Students = new SelectList(students, "Id", "Label", studentId);
			var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
			ViewBag.Courses = new SelectList(courses, "Id", "Title", courseId);
			ViewBag.CoursePrices = courses.ToDictionary(c => c.Id, c => c.Price ?? 0);
		}
	}
}
