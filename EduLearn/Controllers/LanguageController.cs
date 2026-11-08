using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using EduLearn.Localization;

namespace EduLearn.Controllers
{
	// Bangla / English switch (the links in the website header, the login pages and the dashboards)
	public class LanguageController : Controller
	{
		private readonly ITranslator _translator;
		private readonly IWebHostEnvironment _env;

		public LanguageController(ITranslator translator, IWebHostEnvironment env)
		{
			_translator = translator;
			_env = env;
		}

		// GET /Language/Set?culture=bn&returnUrl=/Courses
		public IActionResult Set(string? culture, string? returnUrl)
		{
			var code = string.Equals(culture, "bn", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(culture, "bn-BD", StringComparison.OrdinalIgnoreCase) ? "bn-BD" : "en-US";

			Response.Cookies.Append(Translator.CookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(code)),
				new CookieOptions
				{
					Expires = DateTimeOffset.UtcNow.AddYears(1),
					Path = "/",
					IsEssential = true,
					HttpOnly = true,
					SameSite = SameSiteMode.Lax,
					Secure = Request.IsHttps
				});

			// back to the page the visitor came from (only pages of this site)
			return Url.IsLocalUrl(returnUrl) && !returnUrl.StartsWith("/Language", StringComparison.OrdinalIgnoreCase)
				? LocalRedirect(returnUrl)
				: Redirect("/");
		}

		// Development only: every visible text that had no Bangla translation yet (for finishing the dictionary)
		public IActionResult Missing() => _env.IsDevelopment() ? Json(_translator.Missing()) : NotFound();
	}
}
