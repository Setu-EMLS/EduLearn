using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using EduLearn.Models.Lms;

namespace EduLearn.Infrastructure
{
	public static class UiHelpers
	{
		public const string DefaultCourseImage = "/assets/img/course-1.jpg";
		public const string DefaultAvatar = "/Images/users/student.png";

		// Returns "active" when the current controller (and optionally action) matches, for sidebar links
		public static string ActiveIf(this ViewContext context, string controller, string? action = null)
		{
			var c = context.RouteData.Values["controller"]?.ToString();
			var a = context.RouteData.Values["action"]?.ToString();
			var match = string.Equals(c, controller, StringComparison.OrdinalIgnoreCase)
				&& (action == null || string.Equals(a, action, StringComparison.OrdinalIgnoreCase));
			return match ? "active" : "";
		}

		public static string DashboardLayoutFor(ClaimsPrincipal user) => user.GetRoleName() switch
		{
			"Admin" => "~/Areas/Admin/Views/Shared/_Layout.cshtml",
			"Teacher" => "~/Areas/Teacher/Views/Shared/_Layout.cshtml",
			"Student" => "~/Areas/Student/Views/Shared/_Layout.cshtml",
			_ => "~/Views/Shared/_Layout.cshtml"
		};

		public static string Money(decimal? amount) =>
			(amount ?? 0) <= 0 ? "Free" : $"৳{amount:0,0.##}";

		public static string CourseImage(string? path) => string.IsNullOrEmpty(path) ? DefaultCourseImage : path;

		public static string Avatar(string? path) => string.IsNullOrEmpty(path) ? DefaultAvatar : path;

		public static string StatusBadge(EnrollmentStatus status) => status switch
		{
			EnrollmentStatus.Pending => "bg-warning text-dark",
			EnrollmentStatus.Active => "bg-primary",
			EnrollmentStatus.Completed => "bg-success",
			EnrollmentStatus.Rejected => "bg-danger",
			_ => "bg-secondary"
		};

		public static string LessonIcon(LessonType type) => type switch
		{
			LessonType.Video => "fa-circle-play",
			LessonType.Article => "fa-file-lines",
			LessonType.File => "fa-file-arrow-down",
			_ => "fa-link"
		};

		/// <summary>
		/// Converts a YouTube / Vimeo page URL to an embeddable URL. Returns null when the URL
		/// is not a known video host (direct .mp4 files are handled separately by IsDirectVideo).
		/// </summary>
		public static string? ToEmbedUrl(string? url)
		{
			if (string.IsNullOrWhiteSpace(url)) return null;

			var yt = Regex.Match(url, @"(?:youtube\.com/(?:watch\?(?:.*&)?v=|embed/|shorts/|live/)|youtu\.be/)([A-Za-z0-9_-]{11})");
			if (yt.Success) return $"https://www.youtube.com/embed/{yt.Groups[1].Value}?rel=0";

			var vimeo = Regex.Match(url, @"vimeo\.com/(?:video/)?(\d+)");
			if (vimeo.Success) return $"https://player.vimeo.com/video/{vimeo.Groups[1].Value}";

			var drive = Regex.Match(url, @"drive\.google\.com/file/d/([A-Za-z0-9_-]+)");
			if (drive.Success) return $"https://drive.google.com/file/d/{drive.Groups[1].Value}/preview";

			return null;
		}

		public static bool IsDirectVideo(string? url) =>
			!string.IsNullOrWhiteSpace(url) &&
			Regex.IsMatch(url, @"\.(mp4|webm|ogg)(\?.*)?$", RegexOptions.IgnoreCase);

		// Only http(s) and site-relative links are rendered as clickable hrefs
		public static string? SafeUrl(string? url)
		{
			if (string.IsNullOrWhiteSpace(url)) return null;
			url = url.Trim();
			if (url.StartsWith("/") && !url.StartsWith("//")) return url;
			return Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps)
				? url : null;
		}
	}
}
