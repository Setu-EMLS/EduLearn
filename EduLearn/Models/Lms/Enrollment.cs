using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EduLearn.Areas.Admin.Models;

namespace EduLearn.Models.Lms
{
	public enum EnrollmentStatus
	{
		Pending = 0,
		Active = 1,
		Completed = 2,
		Rejected = 3,
		Cancelled = 4
	}

	public class Enrollment
	{
		public int Id { get; set; }
		public int StudentId { get; set; }
		[ValidateNever]
		public StudentEntity Student { get; set; }
		public int CourseId { get; set; }
		[ValidateNever]
		public Course Course { get; set; }

		public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
		public DateTime EnrolledAt { get; set; } = DateTime.Now;
		public DateTime? ApprovedAt { get; set; }
		public DateTime? CompletedAt { get; set; }

		public decimal Amount { get; set; }
		[StringLength(100)]
		public string? PaymentMethod { get; set; }
		[StringLength(50)]
		public string? SenderNumber { get; set; }
		[StringLength(100)]
		public string? TransactionId { get; set; }
		[StringLength(500)]
		public string? AdminNote { get; set; }

		[StringLength(50)]
		public string? CertificateNo { get; set; }

		[ValidateNever]
		public List<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

		public bool HasAccess => Status == EnrollmentStatus.Active || Status == EnrollmentStatus.Completed;
	}

	public class LessonProgress
	{
		public int Id { get; set; }
		public int EnrollmentId { get; set; }
		public Enrollment Enrollment { get; set; }
		public int LessonId { get; set; }
		public Lesson Lesson { get; set; }
		public DateTime CompletedAt { get; set; } = DateTime.Now;
	}
}
