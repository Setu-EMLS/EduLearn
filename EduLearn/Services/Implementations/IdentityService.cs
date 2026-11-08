using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using EduLearn.Models;
using EduLearn.Services.Interfaces;

namespace EduLearn.Services.Implementations
{
	public class IdentityService : Controller, IIdentityService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public IdentityService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}
		public async Task signInUser(User user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName),
				new Claim(ClaimTypes.Email, user.Email),
				// Role holds the role name so [Authorize(Roles = "Admin")] works
				new Claim(ClaimTypes.Role, RoleIds.Name(user.RoleId)),
				new Claim("RoleId", user.RoleId.ToString()),
				new Claim("RoleName", RoleIds.Name(user.RoleId))
			};

			var identity = new ClaimsIdentity(claims, "MyCookieAuth"); // Match this
			var principal = new ClaimsPrincipal(identity);

			await _httpContextAccessor.HttpContext.SignInAsync("MyCookieAuth", principal); // Match this too
		}

	}
}