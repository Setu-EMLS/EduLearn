using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EduLearn.Areas.Admin.Models
{
	public class Popup
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public List<SitePopup> sitePopups { get; set; }= new List<SitePopup>();
	}
	public class SitePopup
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		[ValidateNever]
		public string PicPath { get; set; }
		[NotMapped]
		public IFormFile Picture { get; set; }
		[ForeignKey("Popup")]
		public int PopupId { get; set; }
		public Popup Popup { get; set; }
	}
}
