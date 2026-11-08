using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class EnrollController : StudentAreaController
	{
		public EnrollController(ApplicationDbContext db, IAccountService accounts) : base(db, accounts)
		{
		}

		// GET /Student/Enroll/Index/{courseId}
		public async Task<IActionResult> Index(int id)
		{
			var course = await Db.Courses
				.Include(c => c.Teacher)
				.Include(c => c.Coursecategory)
				.Include(c => c.Sections).ThenInclude(s => s.Lessons)
				.AsSplitQuery()
				.FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);
			if (course == null) return NotFound();

			var existing = await MyEnrollments().FirstOrDefaultAsync(e => e.CourseId == id);
			if (existing?.HasAccess == true)
				return RedirectToAction("Index", "Learn", new { id });

			ViewBag.Existing = existing;
			ViewBag.IsFull = await IsFullAsync(course.Id, course.Seats);
			ViewBag.Gateways = await Db.PaymentGetways.Include(g => g.GetwayInfo).OrderBy(g => g.Name).ToListAsync();
			return View(course);
		}

		[HttpPost]
		public async Task<IActionResult> Index(int id, string? paymentMethod, string? senderNumber, string? transactionId)
		{
			var course = await Db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);
			if (course == null) return NotFound();

			var existing = await MyEnrollments().FirstOrDefaultAsync(e => e.CourseId == id);
			if (existing?.HasAccess == true)
				return RedirectToAction("Index", "Learn", new { id });
			if (existing?.Status == EnrollmentStatus.Pending)
			{
				TempData["Error"] = "Your request for this course is already waiting for verification.";
				return RedirectToAction("Index", "Courses");
			}
			if (await IsFullAsync(course.Id, course.Seats))
			{
				TempData["Error"] = "Sorry, this course is full.";
				return RedirectToAction(nameof(Index), new { id });
			}

			var enrollment = existing ?? new Enrollment { StudentId = CurrentStudent.Id, CourseId = id };
			enrollment.EnrolledAt = DateTime.Now;
			enrollment.AdminNote = null;
			enrollment.Amount = course.Price ?? 0;

			if (course.IsFree)
			{
				enrollment.Status = EnrollmentStatus.Active;
				enrollment.ApprovedAt = DateTime.Now;
				enrollment.PaymentMethod = "Free";
			}
			else
			{
				transactionId = transactionId?.Trim();
				if (string.IsNullOrWhiteSpace(paymentMethod) || string.IsNullOrWhiteSpace(transactionId))
				{
					TempData["Error"] = "Choose how you paid and enter the transaction ID.";
					return RedirectToAction(nameof(Index), new { id });
				}
				if (await Db.Enrollments.AnyAsync(e => e.TransactionId == transactionId && e.Id != enrollment.Id))
				{
					TempData["Error"] = "This transaction ID has already been used. Please check it and try again.";
					return RedirectToAction(nameof(Index), new { id });
				}

				enrollment.Status = EnrollmentStatus.Pending;
				enrollment.ApprovedAt = null;
				enrollment.PaymentMethod = paymentMethod.Trim();
				enrollment.SenderNumber = senderNumber?.Trim();
				enrollment.TransactionId = transactionId;
			}

			if (existing == null) Db.Enrollments.Add(enrollment);
			await Db.SaveChangesAsync();

			if (enrollment.Status == EnrollmentStatus.Active)
			{
				TempData["Success"] = $"You're enrolled in \"{course.Title}\". Enjoy the course!";
				return RedirectToAction("Index", "Learn", new { id });
			}

			TempData["Success"] = "Payment details submitted. You'll get access as soon as the office verifies your payment.";
			return RedirectToAction("Index", "Courses");
		}

		private async Task<bool> IsFullAsync(int courseId, int? seats)
		{
			if (seats == null || seats <= 0) return false;
			var taken = await Db.Enrollments.CountAsync(e => e.CourseId == courseId &&
				(e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed || e.Status == EnrollmentStatus.Pending));
			return taken >= seats;
		}
	}
}
