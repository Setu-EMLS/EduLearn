using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class EnrollController : StudentAreaController
	{
		private readonly ILearningService _learning;

		public EnrollController(ApplicationDbContext db, IAccountService accounts, ILearningService learning) : base(db, accounts)
		{
			_learning = learning;
		}

		// GET /Student/Enroll/Index/{courseId}?batchId=
		public async Task<IActionResult> Index(int id, int? batchId = null)
		{
			var course = await Db.Courses
				.Include(c => c.Teacher)
				.Include(c => c.Coursecategory)
				.Include(c => c.Sections).ThenInclude(s => s.Lessons)
				.Include(c => c.Batches).ThenInclude(b => b.Teacher)
				.AsSplitQuery()
				.FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);
			if (course == null) return NotFound();

			var existing = await MyEnrollments().FirstOrDefaultAsync(e => e.CourseId == id);
			if (existing?.HasAccess == true)
				return RedirectToAction("Index", "Learn", new { id });

			var openBatches = OpenBatches(course);
			var seatsTaken = await _learning.SeatsTakenAsync(openBatches.Select(b => b.Id));
			var available = openBatches.Where(b => !IsBatchFull(b, seatsTaken)).ToList();

			ViewBag.Existing = existing;
			ViewBag.HasBatches = course.Batches.Any();
			ViewBag.Batches = openBatches;
			ViewBag.SeatsTaken = seatsTaken;
			// Keep the batch picked on the course page (or last time); with one choice, pick it
			ViewBag.SelectedBatchId = available.FirstOrDefault(b => b.Id == (batchId ?? existing?.BatchId))?.Id
				?? (available.Count == 1 ? available[0].Id : (int?)null);
			ViewBag.IsFull = course.Batches.Any()
				? available.Count == 0
				: await IsCourseFullAsync(course.Id, course.Seats);
			ViewBag.Gateways = await Db.PaymentGetways.Include(g => g.GetwayInfo).OrderBy(g => g.Name).ToListAsync();
			return View(course);
		}

		[HttpPost]
		public async Task<IActionResult> Index(int id, int? batchId, string? paymentMethod, string? senderNumber, string? transactionId)
		{
			var course = await Db.Courses
				.Include(c => c.Batches)
				.FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);
			if (course == null) return NotFound();

			var existing = await MyEnrollments().FirstOrDefaultAsync(e => e.CourseId == id);
			if (existing?.HasAccess == true)
				return RedirectToAction("Index", "Learn", new { id });
			if (existing?.Status == EnrollmentStatus.Pending)
			{
				TempData["Error"] = "Your request for this course is already waiting for verification.";
				return RedirectToAction("Index", "Courses");
			}

			Batch? batch = null;
			if (course.Batches.Any())
			{
				batch = OpenBatches(course).FirstOrDefault(b => b.Id == batchId);
				if (batch == null)
				{
					TempData["Error"] = "Please choose one of the open batches.";
					return RedirectToAction(nameof(Index), new { id });
				}
				var taken = await _learning.SeatsTakenAsync(new[] { batch.Id });
				if (IsBatchFull(batch, taken))
				{
					TempData["Error"] = $"Sorry, {batch.Name} is full. Please choose another batch.";
					return RedirectToAction(nameof(Index), new { id });
				}
			}
			else if (await IsCourseFullAsync(course.Id, course.Seats))
			{
				TempData["Error"] = "Sorry, this course is full.";
				return RedirectToAction(nameof(Index), new { id });
			}

			var enrollment = existing ?? new Enrollment { StudentId = CurrentStudent.Id, CourseId = id };
			enrollment.BatchId = batch?.Id;
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
					return RedirectToAction(nameof(Index), new { id, batchId });
				}
				if (await Db.Enrollments.AnyAsync(e => e.TransactionId == transactionId && e.Id != enrollment.Id))
				{
					TempData["Error"] = "This transaction ID has already been used. Please check it and try again.";
					return RedirectToAction(nameof(Index), new { id, batchId });
				}

				enrollment.Status = EnrollmentStatus.Pending;
				enrollment.ApprovedAt = null;
				enrollment.PaymentMethod = paymentMethod.Trim();
				enrollment.SenderNumber = senderNumber?.Trim();
				enrollment.TransactionId = transactionId;
			}

			if (existing == null) Db.Enrollments.Add(enrollment);
			await Db.SaveChangesAsync();

			var where = batch == null ? $"\"{course.Title}\"" : $"\"{course.Title}\" ({batch.Name})";
			if (enrollment.Status == EnrollmentStatus.Active)
			{
				TempData["Success"] = $"You're enrolled in {where}. Enjoy the course!";
				return RedirectToAction("Index", "Learn", new { id });
			}

			TempData["Success"] = $"Payment details for {where} submitted. You'll get access as soon as the office verifies your payment.";
			return RedirectToAction("Index", "Courses");
		}

		private static List<Batch> OpenBatches(Course course) =>
			course.Batches.Where(b => b.AcceptsAdmission).OrderBy(b => b.StartDate).ThenBy(b => b.Name).ToList();

		private static bool IsBatchFull(Batch batch, Dictionary<int, int> seatsTaken) =>
			batch.Seats > 0 && seatsTaken.GetValueOrDefault(batch.Id) >= batch.Seats;

		// Courses without batches keep the course-level seat limit
		private async Task<bool> IsCourseFullAsync(int courseId, int? seats)
		{
			if (seats == null || seats <= 0) return false;
			var taken = await Db.Enrollments.CountAsync(e => e.CourseId == courseId &&
				(e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed || e.Status == EnrollmentStatus.Pending));
			return taken >= seats;
		}
	}
}
