using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
using EduLearn.Models;

namespace EduLearn.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class CourseCategoryController : Controller
	{
		private readonly ApplicationDbContext _context;

		public CourseCategoryController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var categories = await _context.CourseCategorys
				.Include(c => c.Courses)
				.OrderBy(c => c.Title)
				.ToListAsync();
			return View(categories);
		}

		[HttpPost]
		public async Task<IActionResult> Create(CourseCategory model)
		{
			if (!ModelState.IsValid)
			{
				TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
				return RedirectToAction(nameof(Index));
			}

			_context.CourseCategorys.Add(new CourseCategory { Title = model.Title.Trim(), Description = model.Description });
			await _context.SaveChangesAsync();
			TempData["Success"] = "Category added successfully.";
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Edit(int id)
		{
			var category = await _context.CourseCategorys.FindAsync(id);
			if (category == null) return NotFound();
			return View(category);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, CourseCategory model)
		{
			var category = await _context.CourseCategorys.FindAsync(id);
			if (category == null) return NotFound();
			if (!ModelState.IsValid) return View(model);

			category.Title = model.Title.Trim();
			category.Description = model.Description;
			await _context.SaveChangesAsync();
			TempData["Success"] = "Category updated successfully.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var category = await _context.CourseCategorys.Include(c => c.Courses).FirstOrDefaultAsync(c => c.Id == id);
			if (category == null) return NotFound();

			if (category.Courses.Any())
			{
				TempData["Error"] = $"\"{category.Title}\" still has {category.Courses.Count} course(s). Move or delete them first.";
				return RedirectToAction(nameof(Index));
			}

			_context.CourseCategorys.Remove(category);
			await _context.SaveChangesAsync();
			TempData["Success"] = "Category deleted successfully.";
			return RedirectToAction(nameof(Index));
		}
	}
}
