using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
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
                .Include(e => e.Batch)
                .Where(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed)
                .OrderByDescending(e => e.Status == EnrollmentStatus.Active).ThenByDescending(e => e.EnrolledAt)
                .ToListAsync();
            var progress = await _learning.GetProgressAsync(enrollments.Select(e => e.Id));
            var scope = StudentScope.From(enrollments);
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
                DueAssignments = await Db.Assignments.VisibleTo(scope)
                    .Include(a => a.Course)
                    .Where(a => !submittedIds.Contains(a.Id) && (a.DueDate == null || a.DueDate >= now))
                    .OrderBy(a => a.DueDate == null).ThenBy(a => a.DueDate)
                    .Take(5)
                    .ToListAsync(),
                Announcements = await Db.Announcements.VisibleTo(scope)
                    .Include(a => a.Course)
                    .Include(a => a.Batch)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(4)
                    .ToListAsync(),
                UpcomingClasses = await Db.LiveClasses.VisibleTo(scope)
                    .Include(l => l.Course)
                    .Include(l => l.Batch)
                    .Where(l => l.StartsAt.AddMinutes(l.DurationMinutes) >= now)
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
