using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? category = null, string? q = null)
        {
            var query = _context.Courses
                .Include(c => c.Coursecategory)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                .AsSplitQuery()
                .Where(c => c.IsPublished);
            if (category != null) query = query.Where(c => c.CategoryId == category);
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Title.Contains(q) || c.Coursecategory.Title.Contains(q) || (c.ShortDescription != null && c.ShortDescription.Contains(q)));

            ViewBag.Category = category;
            ViewBag.Q = q;
            ViewBag.Categories = await _context.CourseCategorys
                .Where(c => c.Courses.Any(x => x.IsPublished))
                .OrderBy(c => c.Title)
                .ToListAsync();
            return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToAction(nameof(Index));

            var course = await _context.Courses
                .Include(c => c.Coursecategory)
                .Include(c => c.Teacher)
                .Include(c => c.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.SortOrder))
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);
            if (course == null) return NotFound();

            var paid = new[] { EnrollmentStatus.Active, EnrollmentStatus.Completed, EnrollmentStatus.Pending };
            ViewBag.Taken = await _context.Enrollments.CountAsync(e => e.CourseId == course.Id && paid.Contains(e.Status));

            // Tailor the call-to-action for a signed-in student
            if (User.IsInRole("Student"))
            {
                var userId = User.GetUserId();
                ViewBag.MyStatus = await _context.Enrollments
                    .Where(e => e.CourseId == course.Id && e.Student.UserId == userId)
                    .Select(e => (EnrollmentStatus?)e.Status)
                    .FirstOrDefaultAsync();
            }
            return View(course);
        }

        // Free preview lessons are open to everyone
        public async Task<IActionResult> Preview(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Section).ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == id && l.IsPreview && l.Section.Course.IsPublished);
            if (lesson == null) return NotFound();
            return View(lesson);
        }
    }
}
