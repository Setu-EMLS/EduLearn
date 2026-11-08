namespace EduLearn.Services.Interfaces
{
	public enum UploadKind
	{
		Image,
		Document
	}

	public record FileSaveResult(bool Success, string? Path, string? Error)
	{
		public static FileSaveResult Ok(string path) => new(true, path, null);
		public static FileSaveResult Fail(string error) => new(false, null, error);
	}

	public interface IFileService
	{
		/// <summary>Saves an upload under wwwroot/uploads/{folder} and returns its web path.</summary>
		Task<FileSaveResult> SaveAsync(IFormFile file, string folder, UploadKind kind);

		/// <summary>Deletes a file previously returned by SaveAsync. Ignores anything outside /uploads.</summary>
		void Delete(string? webPath);
	}
}
