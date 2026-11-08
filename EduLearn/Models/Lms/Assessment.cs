using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EduLearn.Areas.Admin.Models;

namespace EduLearn.Models.Lms
{
	public class Assignment
	{
		public int Id { get; set; }
		[Display(Name = "Course")]
		public int CourseId { get; set; }
		[ValidateNever]
		public Course Course { get; set; }

		[Required, StringLength(200)]
		public string Title { get; set; }
		public string? Instructions { get; set; }
		[Display(Name = "Due date")]
		public DateTime? DueDate { get; set; }
		[Range(1, 1000)]
		[Display(Name = "Total marks")]
		public int MaxMarks { get; set; } = 100;

		[ValidateNever]
		public string? AttachmentPath { get; set; }
		[NotMapped]
		public IFormFile? Attachment { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[ValidateNever]
		public List<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
	}

	public class AssignmentSubmission
	{
		public int Id { get; set; }
		public int AssignmentId { get; set; }
		public Assignment Assignment { get; set; }
		public int StudentId { get; set; }
		public StudentEntity Student { get; set; }

		public string? Answer { get; set; }
		public string? FilePath { get; set; }
		public DateTime SubmittedAt { get; set; } = DateTime.Now;

		public decimal? Marks { get; set; }
		public string? Feedback { get; set; }
		public DateTime? GradedAt { get; set; }
	}

	public class Quiz
	{
		public int Id { get; set; }
		[Display(Name = "Course")]
		public int CourseId { get; set; }
		[ValidateNever]
		public Course Course { get; set; }

		[Required, StringLength(200)]
		public string Title { get; set; }
		public string? Description { get; set; }
		[Range(1, 600)]
		[Display(Name = "Time limit (minutes)")]
		public int? TimeLimitMinutes { get; set; }
		[Range(0, 100)]
		[Display(Name = "Pass mark (%)")]
		public int PassPercentage { get; set; } = 50;
		[Range(1, 100)]
		[Display(Name = "Max attempts")]
		public int? MaxAttempts { get; set; }
		[Display(Name = "Published")]
		public bool IsPublished { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[ValidateNever]
		public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
		[ValidateNever]
		public List<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
	}

	public class QuizQuestion
	{
		public int Id { get; set; }
		public int QuizId { get; set; }
		public Quiz Quiz { get; set; }
		[Required]
		public string Text { get; set; }
		public int Marks { get; set; } = 1;
		public int SortOrder { get; set; }
		public List<QuizOption> Options { get; set; } = new List<QuizOption>();
	}

	public class QuizOption
	{
		public int Id { get; set; }
		public int QuestionId { get; set; }
		public QuizQuestion Question { get; set; }
		[Required]
		public string Text { get; set; }
		public bool IsCorrect { get; set; }
	}

	public class QuizAttempt
	{
		public int Id { get; set; }
		public int QuizId { get; set; }
		public Quiz Quiz { get; set; }
		public int StudentId { get; set; }
		public StudentEntity Student { get; set; }
		public DateTime StartedAt { get; set; } = DateTime.Now;
		public DateTime? SubmittedAt { get; set; }
		public decimal Score { get; set; }
		public decimal TotalMarks { get; set; }
		public bool Passed { get; set; }
		public List<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();

		[NotMapped]
		public int Percent => TotalMarks > 0 ? (int)Math.Round(Score * 100 / TotalMarks) : 0;
	}

	public class QuizAnswer
	{
		public int Id { get; set; }
		public int AttemptId { get; set; }
		public QuizAttempt Attempt { get; set; }
		public int QuestionId { get; set; }
		public QuizQuestion Question { get; set; }
		public int? SelectedOptionId { get; set; }
		public QuizOption? SelectedOption { get; set; }
		public bool IsCorrect { get; set; }
	}
}
