using EduLearn.Models.Lms;

namespace EduLearn.Infrastructure
{
	/// <summary>
	/// What a student can see: the courses they can open and the batches they belong to.
	/// Rows without a batch are for the whole course; rows with a batch only for that batch.
	/// </summary>
	public sealed class StudentScope
	{
		public List<int> CourseIds { get; init; } = new();
		public List<int> BatchIds { get; init; } = new();

		/// <summary>Build from enrollments that already give access (Active / Completed).</summary>
		public static StudentScope From(IEnumerable<Enrollment> accessible)
		{
			var list = accessible.ToList();
			return new StudentScope
			{
				CourseIds = list.Select(e => e.CourseId).Distinct().ToList(),
				BatchIds = list.Where(e => e.BatchId != null).Select(e => e.BatchId!.Value).Distinct().ToList()
			};
		}
	}

	public static class StudentScopeExtensions
	{
		public static IQueryable<LiveClass> VisibleTo(this IQueryable<LiveClass> query, StudentScope scope) =>
			query.Where(l => scope.CourseIds.Contains(l.CourseId) && (l.BatchId == null || scope.BatchIds.Contains(l.BatchId.Value)));

		public static IQueryable<Assignment> VisibleTo(this IQueryable<Assignment> query, StudentScope scope) =>
			query.Where(a => scope.CourseIds.Contains(a.CourseId) && (a.BatchId == null || scope.BatchIds.Contains(a.BatchId.Value)));

		// Announcements without a course are for everyone
		public static IQueryable<Announcement> VisibleTo(this IQueryable<Announcement> query, StudentScope scope) =>
			query.Where(a => a.CourseId == null
				|| (scope.CourseIds.Contains(a.CourseId.Value) && (a.BatchId == null || scope.BatchIds.Contains(a.BatchId.Value))));
	}
}
