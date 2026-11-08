using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EduLearn.Areas.Admin.Models
{
	public class Career
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
	}

	public class CareerHero
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
	}
}
