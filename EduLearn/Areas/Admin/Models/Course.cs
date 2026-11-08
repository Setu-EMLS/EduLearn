using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EduLearn.Models.Lms;

namespace EduLearn.Areas.Admin.Models
{
	public class CourseCategory
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Category title is required.")]
		[StringLength(100, ErrorMessage = "Title must be between 3 and 100 characters.", MinimumLength = 3)]
		public string Title { get; set; }

		[StringLength(500, ErrorMessage = "Description can be up to 500 characters.")]
		public string? Description { get; set; }

		[ValidateNever]
		public List<Course> Courses { get; set; } = new List<Course>();
	}

	public class Course
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Course title is required")]
		[StringLength(200)]
		public string Title { get; set; }

		[StringLength(300)]
		[Display(Name = "Short Description")]
		public string? ShortDescription { get; set; }

		public string? Description { get; set; }

		[Range(0, 10000000, ErrorMessage = "Price must be zero or more.")]
		[Display(Name = "Price (BDT)")]
		public decimal? Price { get; set; }

		[ForeignKey("Coursecategory")]
		[Display(Name = "Category")]
		[Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
		public int CategoryId { get; set; }

		[ValidateNever]
		public CourseCategory Coursecategory { get; set; }

		[Display(Name = "Teacher")]
		public int? TeacherId { get; set; }

		[ValidateNever]
		public TeacherEntity? Teacher { get; set; }

		[StringLength(50)]
		public string? Level { get; set; }

		[StringLength(50)]
		public string? Duration { get; set; }

		[StringLength(100)]
		public string? Schedule { get; set; }

		[Range(0, 100000)]
		[Display(Name = "Available Seats")]
		public int? Seats { get; set; }

		[ValidateNever]
		public string? ThumbnailPath { get; set; }

		[NotMapped]
		public IFormFile? Thumbnail { get; set; }

		[Display(Name = "Published")]
		public bool IsPublished { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[ValidateNever]
		public List<CourseSection> Sections { get; set; } = new List<CourseSection>();

		[ValidateNever]
		public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

		[NotMapped]
		public bool IsFree => (Price ?? 0) <= 0;
	}

	public class BoosterCategories{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public decimal Icon { get; set; }
	}

	public class FreeSeminar
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public decimal Icon { get; set; }
	}
	public class FreeSeminarBanner
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? PicPath { get; set; }
		public IFormFile Picture { get; set; }
	}
}
