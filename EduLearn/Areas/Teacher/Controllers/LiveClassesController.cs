using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;
using EduLearn.Services.Implementations;
using EduLearn.Services.Interfaces;

namespace EduLearn.Areas.Teacher.Controllers
{
	public class LiveClassesController : TeacherAreaController
	{
		private const long MaxRecordingBytes = FileService.MaxVideoBytes + 10 * 1024 * 1024;

		private readonly IFileService _files;

		public LiveClassesController(ApplicationDbContext db, IFileService files) : base(db)
		{
			_files = files;
		}

		public async Task<IActionResult> Index()
		{
			var courseIds = await ManageableCourseIdsAsync();
			var classes = await Db.LiveClasses
				.Include(l => l.Course)
				.Include(l => l.Batch)
				.Include(l => l.Topics).ThenInclude(t => t.Lesson)
				.Where(l => courseIds.Contains(l.CourseId))
				.OrderByDescending(l => l.StartsAt)
				.ToListAsync();
			return View(classes);
		}

		public async Task<IActionResult> Create(int? courseId = null, int? batchId = null)
		{
			await LoadFormAsync(courseId, null);
			return View("Form", new LiveClass { CourseId = courseId ?? 0, BatchId = batchId });
		}

		[HttpPost]
		public async Task<IActionResult> Create(LiveClass model, int[]? topicIds)
		{
			var topics = (topicIds ?? Array.Empty<int>()).Distinct().ToList();
			await ValidateAsync(model, topics);
			if (!ModelState.IsValid)
			{
				await LoadFormAsync(model.CourseId, topics);
				return View("Form", model);
			}

			var live = new LiveClass();
			Copy(model, live);
			live.Topics = topics.Select(id => new LiveClassTopic { LessonId = id }).ToList();
			Db.LiveClasses.Add(live);
			await Db.SaveChangesAsync();
			TempData["Success"] = "Live class scheduled. Enrolled students will see it in their schedule.";
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Edit(int id)
		{
			var live = await Db.LiveClasses.Include(l => l.Topics).FirstOrDefaultAsync(l => l.Id == id);
			if (live == null || !await CanManageCourseAsync(live.CourseId)) return NotFound();
			await LoadFormAsync(live.CourseId, live.Topics.Select(t => t.LessonId));
			return View("Form", live);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, LiveClass model, int[]? topicIds)
		{
			var live = await Db.LiveClasses.Include(l => l.Topics).FirstOrDefaultAsync(l => l.Id == id);
			if (live == null || !await CanManageCourseAsync(live.CourseId)) return NotFound();

			var topics = (topicIds ?? Array.Empty<int>()).Distinct().ToList();
			await ValidateAsync(model, topics);
			if (!ModelState.IsValid)
			{
				model.Id = id;
				await LoadFormAsync(model.CourseId, topics);
				return View("Form", model);
			}

			Copy(model, live);
			// Keep the links that are still ticked, drop the others, add the new ones
			Db.LiveClassTopics.RemoveRange(live.Topics.Where(t => !topics.Contains(t.LessonId)).ToList());
			foreach (var lessonId in topics.Where(l => live.Topics.All(t => t.LessonId != l)))
				live.Topics.Add(new LiveClassTopic { LessonId = lessonId });
			await Db.SaveChangesAsync();
			TempData["Success"] = "Live class updated.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var live = await Db.LiveClasses.Include(l => l.Topics).FirstOrDefaultAsync(l => l.Id == id);
			if (live == null || !await CanManageCourseAsync(live.CourseId)) return NotFound();
			Db.LiveClassTopics.RemoveRange(live.Topics);
			Db.LiveClasses.Remove(live);
			await Db.SaveChangesAsync();
			_files.Delete(live.RecordingPath);
			TempData["Success"] = "Live class removed.";
			return RedirectToAction(nameof(Index));
		}

		//==================== Recording ====================//

		public async Task<IActionResult> Recording(int id)
		{
			var live = await FindForRecordingAsync(id);
			if (live == null) return NotFound();
			return View(live);
		}

		[HttpPost]
		[RequestSizeLimit(MaxRecordingBytes)]
		[RequestFormLimits(MultipartBodyLengthLimit = MaxRecordingBytes)]
		public async Task<IActionResult> Recording(int id, IFormFile? file, string? url, bool remove = false)
		{
			var live = await FindForRecordingAsync(id);
			if (live == null) return NotFound();

			if (remove)
			{
				_files.Delete(live.RecordingPath);
				live.RecordingPath = null;
				live.RecordingUrl = null;
				live.RecordingAddedAt = null;
				await Db.SaveChangesAsync();
				TempData["Success"] = "Recording removed.";
				return RedirectToAction(nameof(Index));
			}

			url = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
			if (file == null && url == null)
				ModelState.AddModelError(string.Empty, "Choose a video file to upload, or paste a link to the recording.");
			else if (file != null && url != null)
				ModelState.AddModelError(string.Empty, "Use either a video file or a link, not both.");
			else if (url != null && (url.StartsWith('/') || UiHelpers.SafeUrl(url) == null))
				ModelState.AddModelError("url", "Enter a full link starting with https://");

			string? newPath = null;
			if (ModelState.IsValid && file != null)
			{
				var saved = await _files.SaveAsync(file, "recordings", UploadKind.Video);
				if (saved.Success) newPath = saved.Path;
				else ModelState.AddModelError("file", saved.Error!);
			}
			if (!ModelState.IsValid) return View(live);

			// A new recording replaces the old one, whichever way it was added
			var oldPath = live.RecordingPath;
			live.RecordingPath = newPath;
			live.RecordingUrl = url;
			live.RecordingAddedAt = DateTime.Now;
			await Db.SaveChangesAsync();
			_files.Delete(oldPath);

			TempData["Success"] = "Recording saved. Enrolled students can now watch it from their class schedule.";
			return RedirectToAction(nameof(Index));
		}

		private async Task<LiveClass?> FindForRecordingAsync(int id)
		{
			var live = await Db.LiveClasses.Include(l => l.Course).Include(l => l.Batch).FirstOrDefaultAsync(l => l.Id == id);
			return live != null && await CanManageCourseAsync(live.CourseId) ? live : null;
		}

		private async Task ValidateAsync(LiveClass model, List<int> topicIds)
		{
			if (!await CanManageCourseAsync(model.CourseId))
				ModelState.AddModelError(nameof(model.CourseId), "Please select one of your courses.");
			else if (!await IsValidBatchAsync(model.BatchId, model.CourseId))
				ModelState.AddModelError(nameof(model.BatchId), "Choose a batch of the selected course.");
			if (!string.IsNullOrWhiteSpace(model.MeetingUrl) && UiHelpers.SafeUrl(model.MeetingUrl) == null)
				ModelState.AddModelError(nameof(model.MeetingUrl), "Enter a full link starting with https://");
			// Every ticked topic must belong to the chosen course
			if (topicIds.Count > 0 && await Db.Lessons.CountAsync(l => topicIds.Contains(l.Id) && l.Section.CourseId == model.CourseId) != topicIds.Count)
				ModelState.AddModelError("topicIds", "Pick topics from the selected course.");
		}

		private async Task LoadFormAsync(int? courseId, IEnumerable<int>? selectedTopicIds)
		{
			ViewBag.Courses = await CourseSelectListAsync(courseId);
			ViewBag.Batches = await BatchOptionsAsync();

			// Modules and topics of every course the user manages; the form shows the chosen course's
			var courseIds = await ManageableCourseIdsAsync();
			ViewBag.TopicModules = await Db.CourseSections
				.Where(s => courseIds.Contains(s.CourseId))
				.Include(s => s.Lessons)
				.OrderBy(s => s.CourseId).ThenBy(s => s.SortOrder)
				.ToListAsync();
			ViewBag.SelectedTopicIds = (selectedTopicIds ?? Array.Empty<int>()).ToHashSet();
		}

		private static void Copy(LiveClass from, LiveClass to)
		{
			to.CourseId = from.CourseId;
			to.BatchId = from.BatchId;
			to.Title = from.Title.Trim();
			to.StartsAt = from.StartsAt;
			to.DurationMinutes = from.DurationMinutes;
			to.MeetingUrl = string.IsNullOrWhiteSpace(from.MeetingUrl) ? null : from.MeetingUrl.Trim();
			to.Notes = from.Notes;
		}
	}
}
