using EduLearn.Areas.Admin.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Models.ViewModels
{
	public class AdminDashboardViewModel
	{
		public int TotalStudents { get; set; }
		public int TotalTeachers { get; set; }
		public int TotalCourses { get; set; }
		public int PublishedCourses { get; set; }
		public int PendingEnrollments { get; set; }
		public int ActiveEnrollments { get; set; }
		public int CompletedEnrollments { get; set; }
		public decimal Revenue { get; set; }
		public int NewInquiries { get; set; }
		public List<Enrollment> RecentEnrollments { get; set; } = new();
		public List<CourseStatRow> TopCourses { get; set; } = new();
	}

	public class CourseStatRow
	{
		public int CourseId { get; set; }
		public string Title { get; set; } = "";
		public string? TeacherName { get; set; }
		public decimal? Price { get; set; }
		public bool IsPublished { get; set; }
		public int Enrolled { get; set; }
		public int Active { get; set; }
		public int Completed { get; set; }
		public int Pending { get; set; }
		public decimal Revenue { get; set; }
		public int AverageProgress { get; set; }
		public int CompletionRate => Enrolled == 0 ? 0 : (int)Math.Round(Completed * 100.0 / Enrolled);
	}

	public class TeacherDashboardViewModel
	{
		public TeacherEntity? Teacher { get; set; }
		public int CourseCount { get; set; }
		public int StudentCount { get; set; }
		public int UngradedCount { get; set; }
		public int QuizCount { get; set; }
		public List<Course> Courses { get; set; } = new();
		public List<AssignmentSubmission> RecentSubmissions { get; set; } = new();
		public List<LiveClass> UpcomingClasses { get; set; } = new();
	}

	public class StudentDashboardViewModel
	{
		public StudentEntity Student { get; set; } = null!;
		public List<EnrolledCourseItem> Courses { get; set; } = new();
		public int PendingEnrollmentCount { get; set; }
		public List<Assignment> DueAssignments { get; set; } = new();
		public List<Announcement> Announcements { get; set; } = new();
		public List<LiveClass> UpcomingClasses { get; set; } = new();
		public int CertificateCount { get; set; }
		public int? AverageQuizPercent { get; set; }
	}

	public class EnrolledCourseItem
	{
		public Enrollment Enrollment { get; set; } = null!;
		public CourseProgress Progress { get; set; } = new(0, 0);
	}
}

namespace EduLearn.Models.ViewModels
{
	public class LearnViewModel
	{
		public EduLearn.Areas.Admin.Models.Course Course { get; set; } = null!;
		public EduLearn.Models.Lms.Enrollment Enrollment { get; set; } = null!;
		public EduLearn.Models.Lms.Lesson? Current { get; set; }
		public EduLearn.Models.Lms.Lesson? Previous { get; set; }
		public EduLearn.Models.Lms.Lesson? Next { get; set; }
		public HashSet<int> CompletedLessonIds { get; set; } = new();
		public EduLearn.Services.Interfaces.CourseProgress Progress { get; set; } = new(0, 0);
		public int QuizCount { get; set; }
		public int AssignmentCount { get; set; }
	}
}
