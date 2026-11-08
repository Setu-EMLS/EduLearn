using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Models.ViewModels;

namespace EduLearn.Areas.Teacher.Controllers
{
    public class HomeController : TeacherAreaController
    {
        public HomeController(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<IActionResult> Index()
        {
            if (IsAdmin)
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            var courseIds = await ManageableCourseIdsAsync();
            var now = DateTime.Now;

            var vm = new TeacherDashboardViewModel
            {
                Teacher = await CurrentTeacherAsync(),
                CourseCount = courseIds.Count,
                StudentCount = await Db.Enrollments
                    .Where(e => courseIds.Contains(e.CourseId) && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed))
                    .Select(e => e.StudentId).Distinct().CountAsync(),
                UngradedCount = await Db.AssignmentSubmissions
                    .CountAsync(s => courseIds.Contains(s.Assignment.CourseId) && s.Marks == null),
                QuizCount = await Db.Quizzes.CountAsync(q => courseIds.Contains(q.CourseId)),
                Courses = await Db.Courses
                    .Where(c => courseIds.Contains(c.Id))
                    .Include(c => c.Enrollments)
                    .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                    .AsSplitQuery()
                    .ToListAsync(),
                RecentSubmissions = await Db.AssignmentSubmissions
                    .Include(s => s.Student)
                    .Include(s => s.Assignment).ThenInclude(a => a.Course)
                    .Where(s => courseIds.Contains(s.Assignment.CourseId))
                    .OrderBy(s => s.Marks == null ? 0 : 1).ThenByDescending(s => s.SubmittedAt)
                    .Take(6)
                    .ToListAsync(),
                UpcomingClasses = await Db.LiveClasses
                    .Include(l => l.Course)
                    .Where(l => courseIds.Contains(l.CourseId) && l.StartsAt.AddMinutes(l.DurationMinutes) >= now)
                    .OrderBy(l => l.StartsAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
