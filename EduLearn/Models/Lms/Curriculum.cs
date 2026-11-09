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
		Link = 3,
		// A syllabus entry with just a title, taught in the classroom (no video or article needed)
		Topic = 4
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

		// Null means every batch of the course
		[Display(Name = "Batch")]
		public int? BatchId { get; set; }
		[ValidateNever]
		public Batch? Batch { get; set; }

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

		// Recording students can watch after the class: an uploaded file, or a link (YouTube, Vimeo, Google Drive...)
		[ValidateNever, StringLength(500)]
		public string? RecordingPath { get; set; }
		[ValidateNever, StringLength(1000)]
		public string? RecordingUrl { get; set; }
		public DateTime? RecordingAddedAt { get; set; }

		[NotMapped]
		public bool HasRecording => !string.IsNullOrEmpty(RecordingPath) || !string.IsNullOrEmpty(RecordingUrl);

		// The curriculum topics this class covers
		[ValidateNever]
		public List<LiveClassTopic> Topics { get; set; } = new List<LiveClassTopic>();

		// Who attended; a student with no row here hasn't been marked yet
		[ValidateNever]
		public List<LiveClassAttendance> Attendances { get; set; } = new List<LiveClassAttendance>();
	}

	public enum AttendanceStatus
	{
		Present = 0,
		Late = 1,
		Absent = 2
	}

	/// <summary>One student's attendance for one live class, marked by the teacher.</summary>
	public class LiveClassAttendance
	{
		public int Id { get; set; }
		public int LiveClassId { get; set; }
		[ValidateNever]
		public LiveClass LiveClass { get; set; }
		public int EnrollmentId { get; set; }
		[ValidateNever]
		public Enrollment Enrollment { get; set; }

		public AttendanceStatus Status { get; set; }
		public DateTime MarkedAt { get; set; } = DateTime.Now;

		/// <summary>Late students still took part in the class.</summary>
		[NotMapped]
		public bool Attended => Status != AttendanceStatus.Absent;
	}

	/// <summary>Links a live class to a topic (Lesson) of its course's curriculum.</summary>
	public class LiveClassTopic
	{
		public int LiveClassId { get; set; }
		[ValidateNever]
		public LiveClass LiveClass { get; set; }
		public int LessonId { get; set; }
		[ValidateNever]
		public Lesson Lesson { get; set; }
	}

	public class Announcement
	{
		public int Id { get; set; }
		// Null means the announcement is for everyone
		[Display(Name = "Course")]
		public int? CourseId { get; set; }
		[ValidateNever]
		public Course? Course { get; set; }

		// With a course: null means every batch of that course
		[Display(Name = "Batch")]
		public int? BatchId { get; set; }
		[ValidateNever]
		public Batch? Batch { get; set; }

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
