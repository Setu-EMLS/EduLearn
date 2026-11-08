using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EduLearn.Areas.Admin.Models;
using EduLearn.Models;

namespace EduLearn.Areas.Teacher.Models
{
	public class Teacher
	{
		public int Id { get; set; }

		//Generate = T + rendom no
		[ValidateNever]
		public string TeacherID { get; set; }

		public int? UserId { get; set; }
		[ValidateNever]
		public User? User { get; set; }

		[Required, StringLength(150)]
		public string Name { get; set; }
		[StringLength(150)]
		public string? Designation { get; set; }
		public string? Description { get; set; }
		public string? Address { get; set; }
		[Required, EmailAddress]
		public string Email { get; set; }
		public string? Contact { get; set; }
		public string? MotherName { get; set; }
		public string? FatherName { get; set; }
		public string? DateOfBirth { get; set; }

		[ValidateNever]
		public string? PhotoPath { get; set; }
		[NotMapped]
		public IFormFile? Photo { get; set; }

		[ValidateNever]
		public List<Course> Courses { get; set; } = new List<Course>();
	}
}
