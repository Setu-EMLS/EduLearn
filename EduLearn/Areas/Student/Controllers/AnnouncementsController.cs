using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
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
			var list = await Db.Announcements.VisibleTo(await AccessScopeAsync())
				.Include(a => a.Course)
				.Include(a => a.Batch)
				.OrderByDescending(a => a.CreatedAt)
				.Take(100)
				.ToListAsync();
			return View(list);
		}
	}
}
