using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Teacher.Controllers
{
	public class CurriculumController : TeacherAreaController
	{
		private const long MaxUploadBytes = 110 * 1024 * 1024;

		private readonly IFileService _files;
		private readonly ILearningService _learning;

		public CurriculumController(ApplicationDbContext db, IFileService files, ILearningService learning) : base(db)
		{
			_files = files;
			_learning = learning;
		}

		// GET /Teacher/Curriculum/Index/{courseId}
		public async Task<IActionResult> Index(int id)
		{
			if (!await CanManageCourseAsync(id)) return NotFound();

			var course = await Db.Courses
				.Include(c => c.Coursecategory)
				.Include(c => c.Teacher)
				.Include(c => c.Sections.OrderBy(s => s.SortOrder))
					.ThenInclude(s => s.Lessons.OrderBy(l => l.SortOrder))
				.AsSplitQuery()
				.FirstAsync(c => c.Id == id);
			return View(course);
		}

		//==================== Sections ====================//

		[HttpPost]
		public async Task<IActionResult> AddSection(int courseId, string title)
		{
			if (!await CanManageCourseAsync(courseId)) return NotFound();
			if (string.IsNullOrWhiteSpace(title))
			{
				TempData["Error"] = "Section title is required.";
				return RedirectToAction(nameof(Index), new { id = courseId });
			}

			var next = (await Db.CourseSections.Where(s => s.CourseId == courseId).MaxAsync(s => (int?)s.SortOrder) ?? 0) + 1;
			Db.CourseSections.Add(new CourseSection { CourseId = courseId, Title = title.Trim(), SortOrder = next });
			await Db.SaveChangesAsync();
			TempData["Success"] = "Section added.";
			return RedirectToAction(nameof(Index), new { id = courseId });
		}

		[HttpPost]
		public async Task<IActionResult> RenameSection(int id, string title)
		{
			var section = await Db.CourseSections.FindAsync(id);
			if (section == null || !await CanManageCourseAsync(section.CourseId)) return NotFound();

			if (!string.IsNullOrWhiteSpace(title))
			{
				section.Title = title.Trim();
				await Db.SaveChangesAsync();
				TempData["Success"] = "Section renamed.";
			}
			return RedirectToAction(nameof(Index), new { id = section.CourseId });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteSection(int id)
		{
			var section = await Db.CourseSections.Include(s => s.Lessons).FirstOrDefaultAsync(s => s.Id == id);
			if (section == null || !await CanManageCourseAsync(section.CourseId)) return NotFound();

			var files = section.Lessons.Select(l => l.AttachmentPath).ToList();
			Db.CourseSections.Remove(section);
			await Db.SaveChangesAsync();
			foreach (var f in files) _files.Delete(f);
			await RefreshCompletionsAsync(section.CourseId);

			TempData["Success"] = "Section and its lessons were deleted.";
			return RedirectToAction(nameof(Index), new { id = section.CourseId });
		}

		[HttpPost]
		public async Task<IActionResult> MoveSection(int id, int direction)
		{
			var section = await Db.CourseSections.FindAsync(id);
			if (section == null || !await CanManageCourseAsync(section.CourseId)) return NotFound();

			var siblings = await Db.CourseSections.Where(s => s.CourseId == section.CourseId).OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToListAsync();
			Reorder(siblings, section, direction, (s, order) => s.SortOrder = order);
			await Db.SaveChangesAsync();
			return RedirectToAction(nameof(Index), new { id = section.CourseId });
		}

		//==================== Lessons ====================//

		public async Task<IActionResult> CreateLesson(int sectionId)
		{
			var section = await Db.CourseSections.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == sectionId);
			if (section == null || !await CanManageCourseAsync(section.CourseId)) return NotFound();

			ViewBag.Section = section;
			return View("LessonForm", new Lesson { SectionId = sectionId, Type = LessonType.Video });
		}

		[HttpPost]
		[RequestSizeLimit(MaxUploadBytes)]
		[RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
		public async Task<IActionResult> CreateLesson(Lesson model)
		{
			var section = await Db.CourseSections.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == model.SectionId);
			if (section == null || !await CanManageCourseAsync(section.CourseId)) return NotFound();

			string? attachment = null;
			if (model.Attachment != null && ModelState.IsValid)
			{
				var saved = await _files.SaveAsync(model.Attachment, "lessons", UploadKind.Document);
				if (saved.Success) attachment = saved.Path;
				else ModelState.AddModelError(nameof(model.Attachment), saved.Error!);
			}
			ValidateLesson(model, attachment);

			if (!ModelState.IsValid)
			{
				_files.Delete(attachment);
				ViewBag.Section = section;
				return View("LessonForm", model);
			}

			var next = (await Db.Lessons.Where(l => l.SectionId == section.Id).MaxAsync(l => (int?)l.SortOrder) ?? 0) + 1;
			var lesson = new Lesson { SectionId = section.Id, SortOrder = next, CreatedAt = DateTime.Now, AttachmentPath = attachment };
			CopyLesson(model, lesson);
			Db.Lessons.Add(lesson);
			await Db.SaveChangesAsync();

			TempData["Success"] = $"Lesson \"{lesson.Title}\" added.";
			return RedirectToAction(nameof(Index), new { id = section.CourseId });
		}

		public async Task<IActionResult> EditLesson(int id)
		{
			var lesson = await Db.Lessons.Include(l => l.Section).ThenInclude(s => s.Course).FirstOrDefaultAsync(l => l.Id == id);
			if (lesson == null || !await CanManageCourseAsync(lesson.Section.CourseId)) return NotFound();

			ViewBag.Section = lesson.Section;
			return View("LessonForm", lesson);
		}

		[HttpPost]
		[RequestSizeLimit(MaxUploadBytes)]
		[RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
		public async Task<IActionResult> EditLesson(int id, Lesson model, bool removeAttachment = false)
		{
			var lesson = await Db.Lessons.Include(l => l.Section).ThenInclude(s => s.Course).FirstOrDefaultAsync(l => l.Id == id);
			if (lesson == null || !await CanManageCourseAsync(lesson.Section.CourseId)) return NotFound();

			string? newAttachment = null;
			if (model.Attachment != null && ModelState.IsValid)
			{
				var saved = await _files.SaveAsync(model.Attachment, "lessons", UploadKind.Document);
				if (saved.Success) newAttachment = saved.Path;
				else ModelState.AddModelError(nameof(model.Attachment), saved.Error!);
			}
			var effectiveAttachment = newAttachment ?? (removeAttachment ? null : lesson.AttachmentPath);
			ValidateLesson(model, effectiveAttachment);

			if (!ModelState.IsValid)
			{
				_files.Delete(newAttachment);
				model.Id = id;
				model.SectionId = lesson.SectionId;
				model.AttachmentPath = lesson.AttachmentPath;
				ViewBag.Section = lesson.Section;
				return View("LessonForm", model);
			}

			CopyLesson(model, lesson);
			if (newAttachment != null || removeAttachment)
			{
				_files.Delete(lesson.AttachmentPath);
				lesson.AttachmentPath = newAttachment;
			}
			await Db.SaveChangesAsync();

			TempData["Success"] = "Lesson updated.";
			return RedirectToAction(nameof(Index), new { id = lesson.Section.CourseId });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteLesson(int id)
		{
			var lesson = await Db.Lessons.Include(l => l.Section).FirstOrDefaultAsync(l => l.Id == id);
			if (lesson == null || !await CanManageCourseAsync(lesson.Section.CourseId)) return NotFound();

			var courseId = lesson.Section.CourseId;
			Db.Lessons.Remove(lesson);
			await Db.SaveChangesAsync();
			_files.Delete(lesson.AttachmentPath);
			await RefreshCompletionsAsync(courseId);

			TempData["Success"] = "Lesson deleted.";
			return RedirectToAction(nameof(Index), new { id = courseId });
		}

		[HttpPost]
		public async Task<IActionResult> MoveLesson(int id, int direction)
		{
			var lesson = await Db.Lessons.Include(l => l.Section).FirstOrDefaultAsync(l => l.Id == id);
			if (lesson == null || !await CanManageCourseAsync(lesson.Section.CourseId)) return NotFound();

			var siblings = await Db.Lessons.Where(l => l.SectionId == lesson.SectionId).OrderBy(l => l.SortOrder).ThenBy(l => l.Id).ToListAsync();
			Reorder(siblings, lesson, direction, (l, order) => l.SortOrder = order);
			await Db.SaveChangesAsync();
			return RedirectToAction(nameof(Index), new { id = lesson.Section.CourseId });
		}

		//==================== Helpers ====================//

		private void ValidateLesson(Lesson model, string? attachmentPath)
		{
			if (!string.IsNullOrWhiteSpace(model.Url) && UiHelpers.SafeUrl(model.Url) == null)
				ModelState.AddModelError(nameof(model.Url), "Enter a full link starting with http:// or https://");

			switch (model.Type)
			{
				case LessonType.Video when string.IsNullOrWhiteSpace(model.Url) && attachmentPath == null:
					ModelState.AddModelError(nameof(model.Url), "Add a video link (YouTube, Vimeo, Google Drive) or upload an .mp4 file.");
					break;
				case LessonType.Link when string.IsNullOrWhiteSpace(model.Url):
					ModelState.AddModelError(nameof(model.Url), "A link lesson needs a URL.");
					break;
				case LessonType.File when attachmentPath == null:
					ModelState.AddModelError(nameof(model.Attachment), "Upload the file for this lesson.");
					break;
				case LessonType.Article when string.IsNullOrWhiteSpace(model.Content):
					ModelState.AddModelError(nameof(model.Content), "Write the article content.");
					break;
			}
		}

		private static void CopyLesson(Lesson from, Lesson to)
		{
			to.Title = from.Title.Trim();
			to.Type = from.Type;
			to.Url = string.IsNullOrWhiteSpace(from.Url) ? null : from.Url.Trim();
			to.Content = from.Content;
			to.DurationMinutes = from.DurationMinutes;
			to.IsPreview = from.IsPreview;
		}

		// Moves item one step up (-1) or down (+1) and renumbers the list 1..n
		private static void Reorder<T>(List<T> items, T item, int direction, Action<T, int> setOrder)
		{
			var index = items.IndexOf(item);
			var target = index + Math.Sign(direction);
			if (index >= 0 && target >= 0 && target < items.Count)
			{
				items[index] = items[target];
				items[target] = item;
			}
			for (var i = 0; i < items.Count; i++) setOrder(items[i], i + 1);
		}

		// Removing lessons can leave a student with every remaining lesson done
		private async Task RefreshCompletionsAsync(int courseId)
		{
			var active = await Db.Enrollments.Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active).ToListAsync();
			foreach (var e in active) await _learning.RefreshCompletionAsync(e);
		}
	}
}
