using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EduLearn.Areas.Admin.Models;

namespace EduLearn.Models.Lms
{
	public enum BatchPhase
	{
		Upcoming = 0,
		Running = 1,
		Finished = 2
	}

	/// <summary>
	/// One intake of a course with its own start date, class times, trainer and seats.
	/// The curriculum and quizzes are shared by every batch of the course; enrollments,
	/// live classes, assignments and announcements can belong to a single batch.
	/// </summary>
	public class Batch
	{
		public int Id { get; set; }

		[Display(Name = "Course")]
		[Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
		public int CourseId { get; set; }
		[ValidateNever]
		public Course Course { get; set; }

		[Required(ErrorMessage = "Batch name is required.")]
		[StringLength(100)]
		[Display(Name = "Batch name")]
		public string Name { get; set; }

		// Null means the course's own teacher takes this batch
		[Display(Name = "Trainer")]
		public int? TeacherId { get; set; }
		[ValidateNever]
		public TeacherEntity? Teacher { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Start date")]
		public DateTime StartDate { get; set; } = DateTime.Today.AddDays(7);

		[DataType(DataType.Date)]
		[Display(Name = "End date")]
		public DateTime? EndDate { get; set; }

		[StringLength(100)]
		[Display(Name = "Class schedule")]
		public string? Schedule { get; set; }

		[Range(1, 100000, ErrorMessage = "Seats must be 1 or more.")]
		[Display(Name = "Seats")]
		public int? Seats { get; set; }

		[Display(Name = "Admission open")]
		public bool IsAdmissionOpen { get; set; } = true;

		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[ValidateNever]
		public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

		[NotMapped]
		public BatchPhase Phase =>
			StartDate.Date > DateTime.Today ? BatchPhase.Upcoming
			: EndDate != null && EndDate.Value.Date < DateTime.Today ? BatchPhase.Finished
			: BatchPhase.Running;

		/// <summary>Students can apply only while admission is open and the batch hasn't finished.</summary>
		[NotMapped]
		public bool AcceptsAdmission => IsAdmissionOpen && Phase != BatchPhase.Finished;
	}
}
