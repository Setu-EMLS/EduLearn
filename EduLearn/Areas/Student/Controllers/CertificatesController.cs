using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class CertificatesController : StudentAreaController
	{
		public CertificatesController(ApplicationDbContext db, IAccountService accounts) : base(db, accounts)
		{
		}

		public async Task<IActionResult> Index()
		{
			var completed = await MyEnrollments()
				.Include(e => e.Course)
				.Where(e => e.Status == EnrollmentStatus.Completed)
				.OrderByDescending(e => e.CompletedAt)
				.ToListAsync();
			return View(completed);
		}

		// "View" is also the name of Controller.View(), hence the separate method name
		[ActionName("View")]
		public async Task<IActionResult> ViewCertificate(int id)
		{
			var enrollment = await MyEnrollments()
				.Include(e => e.Student)
				.Include(e => e.Course).ThenInclude(c => c.Teacher)
				.FirstOrDefaultAsync(e => e.Id == id && e.Status == EnrollmentStatus.Completed);
			if (enrollment == null) return NotFound();

			return View("Certificate", enrollment);
		}
	}
}
