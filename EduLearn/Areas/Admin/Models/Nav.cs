using System.ComponentModel.DataAnnotations.Schema;

namespace EMSLWebSite.Areas.Admin.Models
{
	public class HeaderNav
	{
		public HeaderNav()
		{
			this.childrens = new List<HeaderNavChild>();
		}
		public int Id { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public string? Status { get; set; }
		public List<HeaderNavChild> childrens { get; set; }
	}

	public class HeaderNavChild
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		[ForeignKey("HeaderNav")]
		public int ParentId { get; set; }
		public  HeaderNav HeaderNav { get; set; }
	}

	public class FooterNav
	{
		public FooterNav()
		{
			this.childrens = new List<FooterNavChild>();
		}
		public int Id { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public string? Status { get; set; }

		public List<FooterNavChild> childrens { get; set; }
	}

	public class FooterNavChild
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		[ForeignKey("FooterNav")]
		public int ParentId { get; set; }
		public FooterNav FooterNav { get; set; }
	}
}
