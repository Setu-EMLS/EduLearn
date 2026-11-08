using System.ComponentModel.DataAnnotations;

namespace EduLearn.Models
{
	public enum InquiryType
	{
		Contact = 0,
		Seminar = 1,
		Newsletter = 2,
		Feedback = 3
	}

	/// <summary>Messages sent from the public website: contact form, free seminar sign-ups, newsletter.</summary>
	public class Inquiry
	{
		public int Id { get; set; }
		public InquiryType Type { get; set; }

		[StringLength(150)]
		public string? Name { get; set; }

		[Required(ErrorMessage = "Email is required"), EmailAddress, StringLength(200)]
		public string Email { get; set; } = "";

		[StringLength(30)]
		public string? Phone { get; set; }

		[StringLength(200)]
		public string? Subject { get; set; }

		[StringLength(200)]
		public string? Course { get; set; }

		[StringLength(4000)]
		public string? Message { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public bool IsHandled { get; set; }
	}
}
