namespace EduLearn.Models
{
	public class User
	{
		public int UserId { get; set; }
		public int RoleId { get; set; }
		public int TenantId { get; set; }
		public string? FullName { get; set; }
		public string? MobileNo { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string? Salt { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}

	public static class RoleIds
	{
		public const int Admin = 1;
		public const int Teacher = 2;
		public const int Student = 3;

		public static string Name(int roleId) => roleId switch
		{
			Admin => "Admin",
			Teacher => "Teacher",
			_ => "Student"
		};
	}
}
