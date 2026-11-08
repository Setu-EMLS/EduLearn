using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EduLearn.Areas.Admin.Models
{
	public class Team
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Designation { get; set; }
		public string? ShortBio { get; set; }
		public string? DetailsBio { get; set; }
		public string? Phone { get; set; }
		public string? Email { get; set; }

		public string? FacebookLink { get; set; }
		public string? LinkedinLink { get; set; }
		[ValidateNever]
		public string? ImageUrl { get; set; }
		[NotMapped]
		public IFormFile ImageUrlFile { get; set; }

	}
	public class TeamHero
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		[ValidateNever]
		public string? PicPath { get; set; }
		[NotMapped]
		public IFormFile? Picture { get; set; }
	}

	public class LookingFor
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public string? Btn { get; set; }
		public string? Url { get; set; }
	}
}
