using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class CoursesController : StudentAreaController
	{
		private readonly ILearningService _learning;

		public CoursesController(ApplicationDbContext db, IAccountService accounts, ILearningService learning) : base(db, accounts)
		{
			_learning = learning;
		}

		// My courses (with progress) and my enrollment requests
		public async Task<IActionResult> Index()
		{
			var enrollments = await MyEnrollments()
				.Include(e => e.Course).ThenInclude(c => c.Teacher)
				.Include(e => e.Course).ThenInclude(c => c.Coursecategory)
				.OrderByDescending(e => e.EnrolledAt)
				.ToListAsync();
			ViewBag.Progress = await _learning.GetProgressAsync(enrollments.Where(e => e.HasAccess).Select(e => e.Id));
			return View(enrollments);
		}

		// Course catalog inside the dashboard
		public async Task<IActionResult> Browse(string? q = null, int? categoryId = null)
		{
			var query = Db.Courses
				.Include(c => c.Coursecategory)
				.Include(c => c.Teacher)
				.Include(c => c.Sections).ThenInclude(s => s.Lessons)
				.Where(c => c.IsPublished);
			if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Title.Contains(q) || (c.ShortDescription != null && c.ShortDescription.Contains(q)));
			if (categoryId != null) query = query.Where(c => c.CategoryId == categoryId);

			ViewBag.Q = q;
			ViewBag.CategoryId = categoryId;
			ViewBag.Categories = await Db.CourseCategorys.Where(c => c.Courses.Any(x => x.IsPublished)).OrderBy(c => c.Title).ToListAsync();
			ViewBag.MyStatus = await MyEnrollments().ToDictionaryAsync(e => e.CourseId, e => e.Status);
			return View(await query.OrderByDescending(c => c.CreatedAt).AsSplitQuery().ToListAsync());
		}
	}
}
