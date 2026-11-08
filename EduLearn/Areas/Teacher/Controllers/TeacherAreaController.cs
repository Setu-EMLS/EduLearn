using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduLearn.Areas.Admin.Models;
using EduLearn.Infrastructure;
using EduLearn.Models;

namespace EduLearn.Areas.Teacher.Controllers
{
	/// <summary>
	/// Base for course-authoring controllers. Teachers only see the courses assigned to them;
	/// admins can manage every course from the same screens.
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
			return Db.Courses.Where(c => c.TeacherId == teacherId);
		}

		protected async Task<bool> CanManageCourseAsync(int courseId) =>
			await (await ManageableCoursesAsync()).AnyAsync(c => c.Id == courseId);

		protected async Task<List<int>> ManageableCourseIdsAsync() =>
			await (await ManageableCoursesAsync()).Select(c => c.Id).ToListAsync();

		protected async Task<SelectList> CourseSelectListAsync(int? selected = null) =>
			new(await (await ManageableCoursesAsync()).OrderBy(c => c.Title).ToListAsync(), "Id", "Title", selected);
	}
}
