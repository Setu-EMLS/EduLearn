using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
	public class AnnouncementsController : StudentAreaController
	{
		public AnnouncementsController(ApplicationDbContext db, IAccountService accounts) : base(db, accounts)
		{
		}

		public async Task<IActionResult> Index()
		{
			var courseIds = await AccessibleCourseIdsAsync();
			var list = await Db.Announcements
				.Include(a => a.Course)
				.Where(a => a.CourseId == null || courseIds.Contains(a.CourseId.Value))
				.OrderByDescending(a => a.CreatedAt)
				.Take(100)
				.ToListAsync();
			return View(list);
		}
	}
}
