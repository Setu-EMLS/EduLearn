using EduLearn.Models.Lms;

namespace EduLearn.Services.Interfaces
{
	public record CourseProgress(int Completed, int Total)
	{
		public int Percent => Total == 0 ? 0 : (int)Math.Round(Completed * 100.0 / Total);
	}

	public interface ILearningService
	{
		/// <summary>Lesson completion per enrollment id.</summary>
		Task<Dictionary<int, CourseProgress>> GetProgressAsync(IEnumerable<int> enrollmentIds);

		/// <summary>Records a completed lesson and marks the enrollment Completed once every lesson is done.</summary>
		Task MarkLessonCompleteAsync(Enrollment enrollment, int lessonId);

		/// <summary>Moves an Active enrollment to Completed (and issues a certificate no.) when all lessons are done.</summary>
		Task RefreshCompletionAsync(Enrollment enrollment);
	}
}
