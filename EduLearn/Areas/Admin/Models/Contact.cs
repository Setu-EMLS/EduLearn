using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EduLearn.Areas.Admin.Models
{
	public class ContactHero
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
	}

	public class DefaultContact
	{
		public int Id { get; set; }
		public string? GoogleMap { get; set; }
		public string? AddressTitle { get; set; }
		public string Address { get; set; }
		public string Phone { get; set; }
		public string? CallCenter { get; set; }
		public string? Email { get; set; }

	}
	public class Address
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string address { get; set; }
		public string? Location { get; set; }
		public string? OfficePhone { get; set; }
	}
	public class MediaLink
	{
		public int Id { get; set; }
		public string? Title { get; set; }
		public string? Url { get; set; }
	}

	public class Brand
	{
		public int Id { get; set; }
		[ValidateNever]
		public string? Logo { get; set; }
		public string? TagLine { get; set; }
		[ValidateNever]
		public string? Icon { get; set; }
		[NotMapped]
		public IFormFile Picture1 { get; set; }
		[NotMapped]
		public IFormFile Picture2 { get; set; }

	}
}
