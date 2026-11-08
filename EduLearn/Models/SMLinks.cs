using System.ComponentModel.DataAnnotations.Schema;

namespace EduLearn.Models
{
	public class SMLinks
	{
		public int id { get; set; }
		public string? Link_Title { get; set; }
		public string? Link { get; set; }
		[ForeignKey("information")]
		public int infoID { get; set; }
		public Information information { get; set; }
	}
}
