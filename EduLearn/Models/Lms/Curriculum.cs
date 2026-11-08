using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EduLearn.Areas.Admin.Models;

namespace EduLearn.Models.Lms
{
	public class CourseSection
	{
		public int Id { get; set; }
		public int CourseId { get; set; }
		[ValidateNever]
		public Course Course { get; set; }

		[Required, StringLength(200)]
		public string Title { get; set; }
		public int SortOrder { get; set; }

		[ValidateNever]
		public List<Lesson> Lessons { get; set; } = new List<Lesson>();
	}

	public enum LessonType
	{
		Video = 0,
		Article = 1,
		File = 2,
		Link = 3
	}

	public class Lesson
	{
		public int Id { get; set; }
		public int SectionId { get; set; }
		[ValidateNever]
		public CourseSection Section { get; set; }

		[Required, StringLength(200)]
		public string Title { get; set; }
		public LessonType Type { get; set; }

		// Video URL (YouTube / Vimeo / .mp4) or external link
		[StringLength(1000)]
		[Display(Name = "Video / Link URL")]
		public string? Url { get; set; }

		// Article body or notes shown under the video
		public string? Content { get; set; }

		[ValidateNever]
		public string? AttachmentPath { get; set; }
		[NotMapped]
		public IFormFile? Attachment { get; set; }

		[Range(0, 1000)]
		[Display(Name = "Duration (minutes)")]
		public int? DurationMinutes { get; set; }
		public int SortOrder { get; set; }

		[Display(Name = "Free preview")]
		public bool IsPreview { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}

	public class LiveClass
	{
		public int Id { get; set; }
		[Display(Name = "Course")]
		public int CourseId { get; set; }
		[ValidateNever]
		public Course Course { get; set; }

		[Required, StringLength(200)]
		public string Title { get; set; }
		[Display(Name = "Starts at")]
		public DateTime StartsAt { get; set; } = DateTime.Now.Date.AddDays(1).AddHours(19);
		[Range(5, 600)]
		[Display(Name = "Duration (minutes)")]
		public int DurationMinutes { get; set; } = 60;
		[StringLength(1000), Url]
		[Display(Name = "Meeting link")]
		public string? MeetingUrl { get; set; }
		public string? Notes { get; set; }
	}

	public class Announcement
	{
		public int Id { get; set; }
		// Null means the announcement is for everyone
		[Display(Name = "Course")]
		public int? CourseId { get; set; }
		[ValidateNever]
		public Course? Course { get; set; }

		[Required, StringLength(200)]
		public string Title { get; set; }
		[Required]
		public string Message { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public int? CreatedByUserId { get; set; }
		[ValidateNever]
		public string? CreatedByName { get; set; }
	}
}
