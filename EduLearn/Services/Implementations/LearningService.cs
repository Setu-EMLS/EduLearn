using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
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
					Total = _context.Lessons.Count(l => l.Section.CourseId == e.CourseId && l.Type != LessonType.Topic),
					Done = e.LessonProgresses.Count()
				})
				.ToListAsync();

			return rows.ToDictionary(r => r.Id, r => new CourseProgress(Math.Min(r.Done, r.Total), r.Total));
		}

		public async Task<Dictionary<int, DateTime>> GetCoveredTopicsAsync(int courseId, int? batchId)
		{
			var rows = await _context.LiveClassTopics
				.Where(t => t.LiveClass.CourseId == courseId && (t.LiveClass.BatchId == null || t.LiveClass.BatchId == batchId))
				.Select(t => new { t.LessonId, t.LiveClass.StartsAt, t.LiveClass.DurationMinutes })
				.ToListAsync();

			// A topic counts as covered once a class that includes it has finished
			var now = DateTime.Now;
			return rows
				.Where(r => r.StartsAt.AddMinutes(r.DurationMinutes) <= now)
				.GroupBy(r => r.LessonId)
				.ToDictionary(g => g.Key, g => g.Max(r => r.StartsAt));
		}

		public async Task<Dictionary<int, int>> SeatsTakenAsync(IEnumerable<int> batchIds)
		{
			var ids = batchIds.Distinct().ToList();
			if (ids.Count == 0) return new Dictionary<int, int>();

			var counts = await _context.Enrollments
				.Where(e => e.BatchId != null && ids.Contains(e.BatchId.Value) &&
					(e.Status == EnrollmentStatus.Pending || e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed))
				.GroupBy(e => e.BatchId!.Value)
				.Select(g => new { g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.Key, x => x.Count);
			return ids.ToDictionary(id => id, id => counts.GetValueOrDefault(id));
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

			// Offline courses are completed by the office, never by finishing lessons
			var mode = await _context.Courses.Where(c => c.Id == enrollment.CourseId).Select(c => c.Mode).FirstOrDefaultAsync();
			if (mode == CourseMode.Offline) return;

			var progress = (await GetProgressAsync(new[] { enrollment.Id }))[enrollment.Id];
			if (progress.Total > 0 && progress.Completed >= progress.Total)
				await IssueCertificateAsync(enrollment);
		}

		public async Task IssueCertificateAsync(Enrollment enrollment, DateTime? completedOn = null)
		{
			if (!enrollment.HasAccess) return;

			enrollment.Status = EnrollmentStatus.Completed;
			enrollment.CompletedAt = completedOn?.Date ?? enrollment.CompletedAt ?? DateTime.Now;
			enrollment.CertificateNo ??= $"EDU-{DateTime.Now:yyyy}-{enrollment.Id:D5}";
			await _context.SaveChangesAsync();
		}

		public async Task RevokeCertificateAsync(Enrollment enrollment)
		{
			if (enrollment.Status != EnrollmentStatus.Completed) return;

			enrollment.Status = EnrollmentStatus.Active;
			enrollment.CompletedAt = null;
			enrollment.CertificateNo = null;
			await _context.SaveChangesAsync();
		}
	}
}
