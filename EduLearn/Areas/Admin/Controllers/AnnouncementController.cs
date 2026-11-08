using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class AnnouncementController : Controller
	{
		private readonly ApplicationDbContext _context;

		public AnnouncementController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var list = await _context.Announcements
				.Include(a => a.Course)
				.OrderByDescending(a => a.CreatedAt)
				.ToListAsync();
			ViewBag.Courses = new SelectList(await _context.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title");
			return View(list);
		}

		[HttpPost]
		public async Task<IActionResult> Create(Announcement model)
		{
			if (model.CourseId != null && !await _context.Courses.AnyAsync(c => c.Id == model.CourseId))
				ModelState.AddModelError(nameof(model.CourseId), "Invalid course.");
			if (!ModelState.IsValid)
			{
				TempData["Error"] = "Title and message are required.";
				return RedirectToAction(nameof(Index));
			}

			_context.Announcements.Add(new Announcement
			{
				CourseId = model.CourseId,
				Title = model.Title.Trim(),
				Message = model.Message.Trim(),
				CreatedAt = DateTime.Now,
				CreatedByUserId = User.GetUserId(),
				CreatedByName = User.GetDisplayName()
			});
			await _context.SaveChangesAsync();
			TempData["Success"] = "Announcement published.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var a = await _context.Announcements.FindAsync(id);
			if (a == null) return NotFound();
			_context.Announcements.Remove(a);
			await _context.SaveChangesAsync();
			TempData["Success"] = "Announcement deleted.";
			return RedirectToAction(nameof(Index));
		}
	}
}
