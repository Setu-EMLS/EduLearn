using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduLearn.Models
{
	public class Information
	{
		public Information()
		{
			this.SMLinks = new List<SMLinks>();
		}
		public int Id { get; set; }
		[NotMapped]
		public IFormFile? LogoImg { get; set; }
		[ValidateNever]
		public string? LogoPath { get; set; }
		public string? Phone { get; set; }
		public string? CustomerCareNumber { get; set; }
		public string? Email { get; set; }
		public string? Address { get; set; }
		public string? Location { get; set; }
		public List<SMLinks> SMLinks { get; set; }
	}
}
