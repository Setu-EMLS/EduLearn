namespace EduLearn.Models.ViewModels
{
	/// <summary>Model for the dark banner at the top of inner website pages.</summary>
	public class PageHero
	{
		public PageHero(string title, string? subtitle = null, string? eyebrow = null, params (string Label, string? Url)[] crumbs)
		{
			Title = title;
			Subtitle = subtitle;
			Eyebrow = eyebrow;
			Crumbs = crumbs.Length > 0 ? crumbs : new[] { (title, (string?)null) };
		}

		public string Title { get; }
		public string? Subtitle { get; }
		public string? Eyebrow { get; }
		public (string Label, string? Url)[] Crumbs { get; }
	}
}
