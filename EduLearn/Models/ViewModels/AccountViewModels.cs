using System.ComponentModel.DataAnnotations;

namespace EduLearn.Models.ViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Name is required")]
		[StringLength(150)]
		public string FullName { get; set; } = "";

		[Required(ErrorMessage = "Mobile number is required")]
		[StringLength(20)]
		[RegularExpression(@"^\+?[0-9\- ]{8,20}$", ErrorMessage = "Enter a valid mobile number")]
		public string MobileNo { get; set; } = "";

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		public string Email { get; set; } = "";

		[Required(ErrorMessage = "Password is required")]
		[MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
		public string Password { get; set; } = "";

		[Required(ErrorMessage = "Confirm Password is required")]
		[Compare("Password", ErrorMessage = "Passwords do not match")]
		public string ConfirmPassword { get; set; } = "";

		public string? ReturnUrl { get; set; }
	}

	/// <summary>Used by the admin to create or edit a teacher / student / admin login together with its profile.</summary>
	public class PersonFormViewModel
	{
		public int Id { get; set; }

		[Required, StringLength(150)]
		public string Name { get; set; } = "";

		[Required, EmailAddress]
		public string Email { get; set; } = "";

		[StringLength(20)]
		[Display(Name = "Mobile")]
		public string? Contact { get; set; }

		[StringLength(150)]
		public string? Designation { get; set; }

		public string? Address { get; set; }

		[Display(Name = "Father's name")]
		public string? FatherName { get; set; }

		[Display(Name = "Mother's name")]
		public string? MotherName { get; set; }

		[Display(Name = "Date of birth")]
		public string? DateOfBirth { get; set; }

		[Display(Name = "About")]
		public string? Description { get; set; }

		// Required on create, optional on edit (blank keeps the current password)
		[MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
		[DataType(DataType.Password)]
		public string? Password { get; set; }

		public IFormFile? Photo { get; set; }
		public string? PhotoPath { get; set; }
		public string? Code { get; set; }
	}
}
