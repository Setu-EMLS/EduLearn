namespace EduLearn.Infrastructure
{
	/// <summary>Public contact details shown across the website (header, footer, contact page). Edit them here.</summary>
	public static class SiteInfo
	{
		public const string Name = "EduLearn";
		public const string Tagline = "Learn Today, Lead Tomorrow";

		// Shown on the home page seminar card, e.g. "12 Nov, 7:00 PM". Leave empty to hide it.
		public const string NextSeminar = "";

		public const string Phone = "+8801913168904";
		public const string PhoneDisplay = "+880 1913-168904";
		public const string Phone2 = "+8801771511805";
		public const string Phone2Display = "+880 1771-511805";

		public const string Email = "info@powersoftit.com";
		public const string SupportEmail = "support@powersoftit.com";
		public const string HrEmail = "hr@powersoftit.com";

		public const string AddressLine1 = "567 (2nd floor), Near 285 no pillar, East Kazipara";
		public const string AddressLine2 = "Kazipara, Mirpur, Dhaka-1216";
		public const string OfficeHours = "Sat – Thu, 9:00 AM – 9:00 PM";

		public const string MapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m14!1m8!1m3!1d7301.181973949841!2d90.372881!3d23.797575!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3755c15dac058dd7%3A0x7f39befc210c74b5!2sPowersoftIT!5e0!3m2!1sen!2sbd!4v1748097044229!5m2!1sen!2sbd";

		// Leave a link empty to hide its icon
		public const string FacebookUrl = "";
		public const string YoutubeUrl = "";
		public const string LinkedinUrl = "";
		public const string InstagramUrl = "";

		// Courses offered in the free seminar sign-up form
		public static readonly string[] SeminarCourses =
		{
			"Full Stack Web Development (ASP.NET Core, C# & SQL)",
			"Graphics Design",
			"Digital Marketing",
			"Office Management with AI",
			"Other / Not sure yet"
		};
	}
}
