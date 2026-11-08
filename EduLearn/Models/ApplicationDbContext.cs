using EMSLWebSite.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
using EduLearn.Areas.Student.Models;
using EduLearn.Areas.Teacher.Models;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Tbl_Users { get; set; }
        public DbSet<Roles> tbl_roles { get; set; }
        public DbSet<Information> Information { get; set; }
        public DbSet<SMLinks> SMLinks { get; set; }

		//=======================Hero====================//
		public DbSet<HomeHero> HomeHero { get; set; }
		public DbSet<AboutUsHero> AboutUsHero { get; set; }
		public DbSet<ServiceHero> ServiceHero { get; set; }
		public DbSet<ProductHero> ProductHero { get; set; }
		public DbSet<FeedbackHero> FeedbackHero { get; set; }
		public DbSet<ContactHero> ContactHero { get; set; }


		//=======================Home====================//
		public DbSet<Home> Homes { get; set; }
		public DbSet<HeaderNav> HeaderNavs { get; set; }
		public DbSet<HeaderNavChild> HeaderNavChilds { get; set; }
		public DbSet<Slider> Sliders { get; set; }
		public DbSet<SliderBtn> SliderBtns { get; set; }
		public DbSet<FooterNav> FooterNavs { get; set; }
		public DbSet<FooterNavChild> FooterNavChilds { get; set; }


		//=================Popups================//
		public DbSet<Popup> Popups { get; set; }
		public DbSet<SitePopup> SitePopups { get; set; }


		//=====================AboutUs====================//
		public DbSet<AboutUs> AboutUs { get; set; }
		public DbSet<Counter> Counters { get; set; }
		public DbSet<Mission> Missions { get; set; }
		public DbSet<Vision> Visions { get; set; }
		public DbSet<WhyITBest> WhyITBest { get; set; }
		public DbSet<Career> Careers { get; set; }


		//=====================Course====================//
		public DbSet<Course> Courses { get; set; }
        public DbSet<CourseCategory> CourseCategorys { get; set; }
        public DbSet<BoosterCategories> BoosterCategories { get; set; }


		//=====================Product====================//
		public DbSet<Product> Products { get; set; }
		public DbSet<WhyChooseOurProduct> WhyChooseOurProduct { get; set; }


		//=====================Payment====================//
		public DbSet<Payment> Payments { get; set; }
		public DbSet<PaymentGetway> PaymentGetways { get; set; }


		//=====================Seminar====================//
		public DbSet<Seminar> Seminars { get; set; }
		public DbSet<SeminarCategory> SeminarCategorys { get; set; }


		//=====================Service====================//
		public DbSet<Service> Services { get; set; }
		public DbSet<FAQ> FAQs { get; set; }


		//=====================Student====================//
		public DbSet<Student> Students { get; set; }
		public DbSet<StudentFeedback> StudentFeedbacks { get; set; }

		//=====================Teacher====================//
		public DbSet<Teacher> Teachers { get; set; }

		//=====================LMS====================//
		public DbSet<CourseSection> CourseSections { get; set; }
		public DbSet<Lesson> Lessons { get; set; }
		public DbSet<Enrollment> Enrollments { get; set; }
		public DbSet<LessonProgress> LessonProgresses { get; set; }
		public DbSet<Assignment> Assignments { get; set; }
		public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
		public DbSet<Quiz> Quizzes { get; set; }
		public DbSet<QuizQuestion> QuizQuestions { get; set; }
		public DbSet<QuizOption> QuizOptions { get; set; }
		public DbSet<QuizAttempt> QuizAttempts { get; set; }
		public DbSet<QuizAnswer> QuizAnswers { get; set; }
		public DbSet<Announcement> Announcements { get; set; }
		public DbSet<LiveClass> LiveClasses { get; set; }

		//=====================Website====================//
		public DbSet<Inquiry> Inquiries { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>().HasIndex(u => u.Email);

			modelBuilder.Entity<Course>().Property(c => c.Price).HasPrecision(18, 2);
			modelBuilder.Entity<Course>()
				.HasOne(c => c.Teacher).WithMany(t => t.Courses)
				.HasForeignKey(c => c.TeacherId).OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<TeacherEntity>()
				.HasOne(t => t.User).WithMany()
				.HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<StudentEntity>()
				.HasOne(s => s.User).WithMany()
				.HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Restrict);

			// Curriculum: Course -> Section -> Lesson
			modelBuilder.Entity<CourseSection>()
				.HasOne(s => s.Course).WithMany(c => c.Sections)
				.HasForeignKey(s => s.CourseId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Lesson>()
				.HasOne(l => l.Section).WithMany(s => s.Lessons)
				.HasForeignKey(l => l.SectionId).OnDelete(DeleteBehavior.Cascade);

			// Enrollments are never cascade-deleted with a course or student
			modelBuilder.Entity<Enrollment>().Property(e => e.Amount).HasPrecision(18, 2);
			modelBuilder.Entity<Enrollment>().HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
			modelBuilder.Entity<Enrollment>()
				.HasOne(e => e.Course).WithMany(c => c.Enrollments)
				.HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Enrollment>()
				.HasOne(e => e.Student).WithMany(s => s.Enrollments)
				.HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<LessonProgress>().HasIndex(p => new { p.EnrollmentId, p.LessonId }).IsUnique();
			modelBuilder.Entity<LessonProgress>()
				.HasOne(p => p.Enrollment).WithMany(e => e.LessonProgresses)
				.HasForeignKey(p => p.EnrollmentId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<LessonProgress>()
				.HasOne(p => p.Lesson).WithMany()
				.HasForeignKey(p => p.LessonId).OnDelete(DeleteBehavior.Cascade);

			// Assignments
			modelBuilder.Entity<Assignment>()
				.HasOne(a => a.Course).WithMany()
				.HasForeignKey(a => a.CourseId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<AssignmentSubmission>().Property(s => s.Marks).HasPrecision(8, 2);
			modelBuilder.Entity<AssignmentSubmission>().HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();
			modelBuilder.Entity<AssignmentSubmission>()
				.HasOne(s => s.Assignment).WithMany(a => a.Submissions)
				.HasForeignKey(s => s.AssignmentId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<AssignmentSubmission>()
				.HasOne(s => s.Student).WithMany()
				.HasForeignKey(s => s.StudentId).OnDelete(DeleteBehavior.Restrict);

			// Quizzes
			modelBuilder.Entity<Quiz>()
				.HasOne(q => q.Course).WithMany()
				.HasForeignKey(q => q.CourseId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<QuizQuestion>()
				.HasOne(q => q.Quiz).WithMany(q => q.Questions)
				.HasForeignKey(q => q.QuizId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<QuizOption>()
				.HasOne(o => o.Question).WithMany(q => q.Options)
				.HasForeignKey(o => o.QuestionId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<QuizAttempt>().Property(a => a.Score).HasPrecision(8, 2);
			modelBuilder.Entity<QuizAttempt>().Property(a => a.TotalMarks).HasPrecision(8, 2);
			modelBuilder.Entity<QuizAttempt>()
				.HasOne(a => a.Quiz).WithMany(q => q.Attempts)
				.HasForeignKey(a => a.QuizId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<QuizAttempt>()
				.HasOne(a => a.Student).WithMany()
				.HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<QuizAnswer>()
				.HasOne(a => a.Attempt).WithMany(a => a.Answers)
				.HasForeignKey(a => a.AttemptId).OnDelete(DeleteBehavior.Cascade);
			// Second paths into QuizAnswer must not cascade (SQL Server rejects multiple cascade paths)
			modelBuilder.Entity<QuizAnswer>()
				.HasOne(a => a.Question).WithMany()
				.HasForeignKey(a => a.QuestionId).OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<QuizAnswer>()
				.HasOne(a => a.SelectedOption).WithMany()
				.HasForeignKey(a => a.SelectedOptionId).OnDelete(DeleteBehavior.NoAction);

			// Announcements / live classes
			modelBuilder.Entity<Announcement>()
				.HasOne(a => a.Course).WithMany()
				.HasForeignKey(a => a.CourseId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<LiveClass>()
				.HasOne(l => l.Course).WithMany()
				.HasForeignKey(l => l.CourseId).OnDelete(DeleteBehavior.Cascade);
		}
	}
}