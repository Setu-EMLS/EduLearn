using System.ComponentModel.DataAnnotations.Schema;

namespace EduLearn.Areas.Admin.Models
{
	public class Payment
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
	}

	public class PaymentGetway
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; } = null;
		public List<GetwayInfo> GetwayInfo { get; set; } = new List<GetwayInfo>();
	}

	public class GetwayInfo
	{
		public int Id { get; set; } 
		public string Name { get; set; }
		public string? No { get; set; }
		public string? Description { get; set; }
		public string? Branch { get; set; }
		[ForeignKey("Getway")]
		public int? GetwayId { get; set; }
		public PaymentGetway Getway { get; set; }
	}
}
