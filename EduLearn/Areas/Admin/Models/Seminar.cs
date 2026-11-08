using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EduLearn.Areas.Admin.Models
{
	public class SeminarCategory
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
	}

	public class Seminar
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		[ValidateNever]
		public string Picpath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }

		public string? Btn { get; set; }
		public string? Url { get; set; }
	}
}
