using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EduLearn.Areas.Admin.Models
{
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public string? Category { get; set; }
		[ValidateNever]
		public string? Picpath {  get; set; }
		[NotMapped]
		public IFormFile? ProductName { get; set; }
	}
	public class WhyChooseOurProduct
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
	}
}
