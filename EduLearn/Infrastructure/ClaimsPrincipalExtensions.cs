using System.Security.Claims;

namespace EduLearn.Infrastructure
{
	public static class ClaimsPrincipalExtensions
	{
		public static int GetUserId(this ClaimsPrincipal user)
		{
			var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
			return int.TryParse(value, out var id) ? id : 0;
		}

		public static string? GetEmail(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email);

		public static string GetDisplayName(this ClaimsPrincipal user) =>
			user.Identity?.Name ?? user.GetEmail() ?? "User";

		public static string? GetRoleName(this ClaimsPrincipal user) => user.FindFirstValue("RoleName");

		// Where the "Dashboard" link should take a signed-in user
		public static string DashboardUrl(this ClaimsPrincipal user) => user.GetRoleName() switch
		{
			"Admin" => "/Admin",
			"Teacher" => "/Teacher",
			"Student" => "/Student",
			_ => "/"
		};
	}
}
