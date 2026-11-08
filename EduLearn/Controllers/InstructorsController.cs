using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;

namespace EduLearn.Controllers
{
    // Public list of the teachers (instructors) with an active account
    public class InstructorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstructorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var instructors = await _context.Teachers
                .Where(t => t.User == null || t.User.IsActive)
                .Include(t => t.Courses).ThenInclude(c => c.Enrollments)
                .OrderBy(t => t.Name)
                .AsSplitQuery()
                .ToListAsync();
            return View(instructors);
        }
    }
}
