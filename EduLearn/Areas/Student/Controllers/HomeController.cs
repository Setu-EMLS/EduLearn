using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Models.ViewModels;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Student.Controllers
{
    public class HomeController : StudentAreaController
    {
        private readonly ILearningService _learning;

        public HomeController(ApplicationDbContext db, IAccountService accounts, ILearningService learning) : base(db, accounts)
        {
            _learning = learning;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var enrollments = await MyEnrollments()
                .Include(e => e.Course).ThenInclude(c => c.Teacher)
                .Where(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed)
                .OrderByDescending(e => e.Status == EnrollmentStatus.Active).ThenByDescending(e => e.EnrolledAt)
                .ToListAsync();
            var progress = await _learning.GetProgressAsync(enrollments.Select(e => e.Id));
            var courseIds = enrollments.Select(e => e.CourseId).ToList();
            var submittedIds = await Db.AssignmentSubmissions
                .Where(s => s.StudentId == CurrentStudent.Id)
                .Select(s => s.AssignmentId)
                .ToListAsync();
            var attempts = await Db.QuizAttempts
                .Where(a => a.StudentId == CurrentStudent.Id && a.SubmittedAt != null && a.TotalMarks > 0)
                .Select(a => new { a.Score, a.TotalMarks })
                .ToListAsync();

            var vm = new StudentDashboardViewModel
            {
                Student = CurrentStudent,
                Courses = enrollments.Select(e => new EnrolledCourseItem { Enrollment = e, Progress = progress[e.Id] }).ToList(),
                PendingEnrollmentCount = await MyEnrollments().CountAsync(e => e.Status == EnrollmentStatus.Pending),
                DueAssignments = await Db.Assignments
                    .Include(a => a.Course)
                    .Where(a => courseIds.Contains(a.CourseId) && !submittedIds.Contains(a.Id) && (a.DueDate == null || a.DueDate >= now))
                    .OrderBy(a => a.DueDate == null).ThenBy(a => a.DueDate)
                    .Take(5)
                    .ToListAsync(),
                Announcements = await Db.Announcements
                    .Include(a => a.Course)
                    .Where(a => a.CourseId == null || courseIds.Contains(a.CourseId.Value))
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(4)
                    .ToListAsync(),
                UpcomingClasses = await Db.LiveClasses
                    .Include(l => l.Course)
                    .Where(l => courseIds.Contains(l.CourseId) && l.StartsAt.AddMinutes(l.DurationMinutes) >= now)
                    .OrderBy(l => l.StartsAt)
                    .Take(4)
                    .ToListAsync(),
                CertificateCount = enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
                AverageQuizPercent = attempts.Any() ? (int)Math.Round(attempts.Average(a => a.Score * 100 / a.TotalMarks)) : null
            };

            return View(vm);
        }
    }
}
