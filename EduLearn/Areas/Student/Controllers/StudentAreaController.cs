using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	[Area("Student")]
	[Authorize(Roles = "Student")]
	public abstract class StudentAreaController : Controller
	{
		protected readonly ApplicationDbContext Db;
		private readonly IAccountService _accounts;

		protected StudentEntity CurrentStudent { get; private set; } = null!;

		protected StudentAreaController(ApplicationDbContext db, IAccountService accounts)
		{
			Db = db;
			_accounts = accounts;
		}

		// Load (or lazily create) the student's profile before every action
		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var student = await _accounts.GetOrCreateStudentProfileAsync(User.GetUserId());
			if (student == null)
			{
				context.Result = RedirectToAction("Logout", "Home", new { area = "" });
				return;
			}
			CurrentStudent = student;
			await next();
		}

		protected IQueryable<Enrollment> MyEnrollments() => Db.Enrollments.Where(e => e.StudentId == CurrentStudent.Id);

		/// <summary>Courses the student can currently open (Active or Completed enrollments).</summary>
		protected async Task<List<int>> AccessibleCourseIdsAsync() =>
			await MyEnrollments()
				.Where(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed)
				.Select(e => e.CourseId)
				.ToListAsync();

		protected async Task<Enrollment?> AccessibleEnrollmentAsync(int courseId) =>
			await MyEnrollments().FirstOrDefaultAsync(e =>
				e.CourseId == courseId && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed));
	}
}
