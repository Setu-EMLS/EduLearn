using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Models.ViewModels;

namespace EduLearn.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public HomeController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var paid = new[] { EnrollmentStatus.Active, EnrollmentStatus.Completed };

			var vm = new AdminDashboardViewModel
			{
				TotalStudents = await _context.Students.CountAsync(),
				TotalTeachers = await _context.Teachers.CountAsync(),
				TotalCourses = await _context.Courses.CountAsync(),
				PublishedCourses = await _context.Courses.CountAsync(c => c.IsPublished),
				PendingEnrollments = await _context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Pending),
				ActiveEnrollments = await _context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Active),
				CompletedEnrollments = await _context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Completed),
				Revenue = await _context.Enrollments.Where(e => paid.Contains(e.Status)).SumAsync(e => (decimal?)e.Amount) ?? 0,
				NewInquiries = await _context.Inquiries.CountAsync(i => !i.IsHandled && i.Type != InquiryType.Newsletter),
				RecentEnrollments = await _context.Enrollments
					.Include(e => e.Student)
					.Include(e => e.Course)
					.OrderByDescending(e => e.EnrolledAt)
					.Take(8)
					.ToListAsync(),
				TopCourses = await _context.Courses
					.Select(c => new CourseStatRow
					{
						CourseId = c.Id,
						Title = c.Title,
						TeacherName = c.Teacher != null ? c.Teacher.Name : null,
						IsPublished = c.IsPublished,
						Enrolled = c.Enrollments.Count(e => paid.Contains(e.Status)),
						Pending = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Pending)
					})
					.OrderByDescending(c => c.Enrolled)
					.Take(5)
					.ToListAsync()
			};

			return View(vm);
		}
	}
}
