using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Areas.Student.Models
{
	public class Student
	{
		public int Id { get; set; }
		//Generate = ST + year/month + rendom no
		[ValidateNever]
		public string StudentID {  get; set; }

		public int? UserId { get; set; }
		[ValidateNever]
		public User? User { get; set; }

		[Required, StringLength(150)]
		public string Name { get; set; }
		public string? Description { get; set; }
		public string? Address { get; set; }
		[Required, EmailAddress]
		public string Email {  get; set; }
		public string? Contact {  get; set; }
		public string? MotherName {  get; set; }
		public string? FatherName {  get; set; }
		public string? DateOfBirth {  get; set; }

		[ValidateNever]
		public string? PhotoPath { get; set; }
		[NotMapped]
		public IFormFile? Photo { get; set; }

		[ValidateNever]
		public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
	}
}
