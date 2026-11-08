using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
using EduLearn.Infrastructure;
using EduLearn.Models;
using EduLearn.Models.Lms;

namespace EduLearn.Areas.Teacher.Controllers
{
	/// <summary>
	/// Base for course-authoring controllers. Teachers only see the courses assigned to them
	/// (as the course teacher or as the trainer of one of its batches); admins can manage every
	/// course from the same screens.
	/// </summary>
	[Area("Teacher")]
	[Authorize(Roles = "Teacher,Admin")]
	public abstract class TeacherAreaController : Controller
	{
		protected readonly ApplicationDbContext Db;
		private TeacherEntity? _teacher;
		private bool _teacherLoaded;

		protected TeacherAreaController(ApplicationDbContext db)
		{
			Db = db;
		}

		protected bool IsAdmin => User.IsInRole("Admin");

		protected async Task<TeacherEntity?> CurrentTeacherAsync()
		{
			if (!_teacherLoaded)
			{
				var userId = User.GetUserId();
				_teacher = await Db.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
				_teacherLoaded = true;
			}
			return _teacher;
		}

		protected async Task<IQueryable<Course>> ManageableCoursesAsync()
		{
			if (IsAdmin) return Db.Courses;
			var teacherId = (await CurrentTeacherAsync())?.Id ?? -1;
			return Db.Courses.Where(c => c.TeacherId == teacherId || c.Batches.Any(b => b.TeacherId == teacherId));
		}

		protected async Task<bool> CanManageCourseAsync(int courseId) =>
			await (await ManageableCoursesAsync()).AnyAsync(c => c.Id == courseId);

		protected async Task<List<int>> ManageableCourseIdsAsync() =>
			await (await ManageableCoursesAsync()).Select(c => c.Id).ToListAsync();

		protected async Task<SelectList> CourseSelectListAsync(int? selected = null) =>
			new(await (await ManageableCoursesAsync()).OrderBy(c => c.Title).ToListAsync(), "Id", "Title", selected);

		/// <summary>Batches of the manageable courses, for the Batch drop-downs (they filter by course in the browser).</summary>
		protected async Task<List<Batch>> BatchOptionsAsync()
		{
			var courseIds = await ManageableCourseIdsAsync();
			return await Db.Batches
				.Where(b => courseIds.Contains(b.CourseId))
				.OrderByDescending(b => b.StartDate).ThenBy(b => b.Name)
				.ToListAsync();
		}

		/// <summary>No batch (= every batch) or a batch of the chosen course.</summary>
		protected async Task<bool> IsValidBatchAsync(int? batchId, int courseId) =>
			batchId == null || await Db.Batches.AnyAsync(b => b.Id == batchId && b.CourseId == courseId);
	}
}
