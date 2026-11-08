using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EduLearn.Areas.Admin.Models
{
    public class Hero
    {
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
	}
	public class HomeHero
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Button { get; set; }
		public string Link { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
	}
	public class AboutUsHero
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
	}
	public class ServiceHero
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
	}
	public class ProductHero
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
	}
	public class FeedbackHero
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
	}
	public class Contact
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
	}
}
