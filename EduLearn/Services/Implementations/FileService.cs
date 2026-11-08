using EduLearn.Services.Interfaces;

namespace EduLearn.Services.Implementations
{
	public class FileService : IFileService
	{
		private const string UploadRoot = "uploads";

		private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".jpg", ".jpeg", ".png", ".webp", ".gif"
		};

		private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".csv",
			".zip", ".rar", ".7z", ".jpg", ".jpeg", ".png", ".mp4", ".mp3"
		};

		private const long MaxImageBytes = 5 * 1024 * 1024;
		private const long MaxDocumentBytes = 100 * 1024 * 1024;

		private readonly IWebHostEnvironment _env;

		public FileService(IWebHostEnvironment env)
		{
			_env = env;
		}

		public async Task<FileSaveResult> SaveAsync(IFormFile file, string folder, UploadKind kind)
		{
			if (file == null || file.Length == 0)
				return FileSaveResult.Fail("The selected file is empty.");

			var ext = Path.GetExtension(file.FileName);
			var allowed = kind == UploadKind.Image ? ImageExtensions : DocumentExtensions;
			var maxBytes = kind == UploadKind.Image ? MaxImageBytes : MaxDocumentBytes;

			if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))
				return FileSaveResult.Fail($"File type {ext} is not allowed. Allowed: {string.Join(", ", allowed)}");
			if (file.Length > maxBytes)
				return FileSaveResult.Fail($"File is too large. Maximum size is {maxBytes / (1024 * 1024)} MB.");

			// Folder names come from code, but keep them to safe characters anyway
			var safeFolder = new string(folder.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
			var dir = Path.Combine(_env.WebRootPath, UploadRoot, safeFolder);
			Directory.CreateDirectory(dir);

			var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
			await using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.CreateNew))
			{
				await file.CopyToAsync(stream);
			}

			return FileSaveResult.Ok($"/{UploadRoot}/{safeFolder}/{fileName}");
		}

		public void Delete(string? webPath)
		{
			if (string.IsNullOrEmpty(webPath) || !webPath.StartsWith($"/{UploadRoot}/"))
				return;

			var root = Path.GetFullPath(Path.Combine(_env.WebRootPath, UploadRoot));
			var full = Path.GetFullPath(Path.Combine(_env.WebRootPath, webPath.TrimStart('/')));
			if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(full))
				File.Delete(full);
		}
	}
}
