using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Services.Implementations
{
	public class LearningService : ILearningService
	{
		private readonly ApplicationDbContext _context;

		public LearningService(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Dictionary<int, CourseProgress>> GetProgressAsync(IEnumerable<int> enrollmentIds)
		{
			var ids = enrollmentIds.Distinct().ToList();
			if (ids.Count == 0) return new Dictionary<int, CourseProgress>();

			var rows = await _context.Enrollments
				.Where(e => ids.Contains(e.Id))
				.Select(e => new
				{
					e.Id,
					Total = _context.Lessons.Count(l => l.Section.CourseId == e.CourseId),
					Done = e.LessonProgresses.Count()
				})
				.ToListAsync();

			return rows.ToDictionary(r => r.Id, r => new CourseProgress(Math.Min(r.Done, r.Total), r.Total));
		}

		public async Task MarkLessonCompleteAsync(Enrollment enrollment, int lessonId)
		{
			var exists = await _context.LessonProgresses
				.AnyAsync(p => p.EnrollmentId == enrollment.Id && p.LessonId == lessonId);
			if (!exists)
			{
				_context.LessonProgresses.Add(new LessonProgress { EnrollmentId = enrollment.Id, LessonId = lessonId });
				await _context.SaveChangesAsync();
			}

			await RefreshCompletionAsync(enrollment);
		}

		public async Task RefreshCompletionAsync(Enrollment enrollment)
		{
			if (enrollment.Status != EnrollmentStatus.Active) return;

			var progress = (await GetProgressAsync(new[] { enrollment.Id }))[enrollment.Id];
			if (progress.Total > 0 && progress.Completed >= progress.Total)
			{
				enrollment.Status = EnrollmentStatus.Completed;
				enrollment.CompletedAt = DateTime.Now;
				enrollment.CertificateNo ??= $"EDU-{DateTime.Now:yyyy}-{enrollment.Id:D5}";
				await _context.SaveChangesAsync();
			}
		}
	}
}
