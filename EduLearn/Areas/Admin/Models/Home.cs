using System.ComponentModel.DataAnnotations.Schema;

namespace EduLearn.Areas.Admin.Models
{
	public class Home	{
		public int Id { get; set; }
		public string? SectionName { get; set; }
	}

	public class Slider
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }

		// Navigation property - one Slider can have many Buttons
		public ICollection<SliderBtn> SliderBtns { get; set; } = new List<SliderBtn>();
	}

	public class SliderBtn
	{
		public int Id { get; set; } // ✅ Recommended to add a primary key for this table

		public string BtnText { get; set; }
		public string? BtnUrl { get; set; }

		// Foreign key
		public int SliderId { get; set; }

		// Navigation property
		public Slider Slider { get; set; }
	}


}
