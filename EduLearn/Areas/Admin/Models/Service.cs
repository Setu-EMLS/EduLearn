using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EduLearn.Areas.Admin.Models
{
	public class Service
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string? Icon { get; set; }
		
	}
}
