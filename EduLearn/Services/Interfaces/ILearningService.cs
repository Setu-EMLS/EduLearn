using EduLearn.Models.Lms;

namespace EduLearn.Services.Interfaces
{
	public record CourseProgress(int Completed, int Total)
	{
		public int Percent => Total == 0 ? 0 : (int)Math.Round(Completed * 100.0 / Total);
	}

	public interface ILearningService
	{
		/// <summary>Topic completion per enrollment id. Class-only topics are taught in class, so they are not counted here.</summary>
		Task<Dictionary<int, CourseProgress>> GetProgressAsync(IEnumerable<int> enrollmentIds);

		/// <summary>
		/// Topics already taught in a finished live class that a student of this course (and batch) attended:
		/// topic id -> date of its most recent class.
		/// </summary>
		Task<Dictionary<int, DateTime>> GetCoveredTopicsAsync(int courseId, int? batchId);

		/// <summary>Records a completed lesson and marks the enrollment Completed once every lesson is done.</summary>
		Task MarkLessonCompleteAsync(Enrollment enrollment, int lessonId);

		/// <summary>Moves an Active enrollment to Completed (and issues a certificate no.) when all lessons are done. Offline courses are never completed this way.</summary>
		Task RefreshCompletionAsync(Enrollment enrollment);

		/// <summary>Marks an enrollment that has access as Completed and gives it a certificate number. The office uses this for offline courses.</summary>
		Task IssueCertificateAsync(Enrollment enrollment, DateTime? completedOn = null);

		/// <summary>Takes a certificate back: the enrollment goes back to Active and loses its certificate number.</summary>
		Task RevokeCertificateAsync(Enrollment enrollment);

		/// <summary>Seats held per batch id. Pending requests hold a seat while the payment is checked.</summary>
		Task<Dictionary<int, int>> SeatsTakenAsync(IEnumerable<int> batchIds);
	}
}
