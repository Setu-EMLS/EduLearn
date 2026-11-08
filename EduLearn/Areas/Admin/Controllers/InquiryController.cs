using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;

namespace EduLearn.Areas.Admin.Controllers
{
	// Messages from the website: contact form, free seminar sign-ups, newsletter subscribers
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class InquiryController : Controller
	{
		private readonly ApplicationDbContext _context;

		public InquiryController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(InquiryType? type = null)
		{
			var list = await _context.Inquiries
				.Where(i => type == null || i.Type == type)
				.OrderBy(i => i.IsHandled)
				.ThenByDescending(i => i.CreatedAt)
				.Take(1000)
				.ToListAsync();

			ViewBag.Type = type;
			ViewBag.Counts = await _context.Inquiries
				.GroupBy(i => i.Type)
				.Select(g => new { g.Key, New = g.Count(i => !i.IsHandled), Total = g.Count() })
				.ToDictionaryAsync(x => x.Key, x => (x.New, x.Total));
			return View(list);
		}

		[HttpPost]
		public async Task<IActionResult> ToggleHandled(int id, InquiryType? returnType)
		{
			var inquiry = await _context.Inquiries.FindAsync(id);
			if (inquiry == null) return NotFound();

			inquiry.IsHandled = !inquiry.IsHandled;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index), new { type = returnType });
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id, InquiryType? returnType)
		{
			var inquiry = await _context.Inquiries.FindAsync(id);
			if (inquiry == null) return NotFound();

			_context.Inquiries.Remove(inquiry);
			await _context.SaveChangesAsync();
			TempData["Success"] = "Message deleted.";
			return RedirectToAction(nameof(Index), new { type = returnType });
		}
	}
}
