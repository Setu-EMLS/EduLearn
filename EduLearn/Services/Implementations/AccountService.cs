using Microsoft.EntityFrameworkCore;
using EduLearn.Models;
using EduLearn.Services.Interfaces;

namespace EduLearn.Services.Implementations
{
	public class AccountService : IAccountService
	{
		private readonly ApplicationDbContext _context;
		private readonly IUserService _userService;

		public AccountService(ApplicationDbContext context, IUserService userService)
		{
			_context = context;
			_userService = userService;
		}

		public bool EmailExists(string email, int? exceptUserId = null)
		{
			var normalized = email.Trim();
			return _context.Tbl_Users.Any(u => u.Email == normalized && (exceptUserId == null || u.UserId != exceptUserId));
		}

		public void SetPassword(User user, string password)
		{
			user.Salt = _userService.CreateSaltKey(16);
			user.Password = _userService.CreatePasswordHash(password, user.Salt);
		}

		private User NewUser(int roleId, string name, string email, string? phone, string password)
		{
			var user = new User
			{
				RoleId = roleId,
				TenantId = 0,
				FullName = name.Trim(),
				Email = email.Trim(),
				MobileNo = phone?.Trim(),
				IsActive = true,
				CreatedAt = DateTime.Now
			};
			SetPassword(user, password);
			return user;
		}

		public async Task<StudentEntity> CreateStudentAsync(string name, string email, string? phone, string password)
		{
			var user = NewUser(RoleIds.Student, name, email, phone, password);
			var student = new StudentEntity
			{
				User = user,
				StudentID = await NewStudentCodeAsync(),
				Name = user.FullName!,
				Email = user.Email,
				Contact = user.MobileNo
			};
			_context.Students.Add(student);
			await _context.SaveChangesAsync();
			return student;
		}

		public async Task<TeacherEntity> CreateTeacherAsync(string name, string email, string? phone, string password, string? designation = null)
		{
			var user = NewUser(RoleIds.Teacher, name, email, phone, password);
			var teacher = new TeacherEntity
			{
				User = user,
				TeacherID = await NewTeacherCodeAsync(),
				Name = user.FullName!,
				Email = user.Email,
				Contact = user.MobileNo,
				Designation = designation
			};
			_context.Teachers.Add(teacher);
			await _context.SaveChangesAsync();
			return teacher;
		}

		public async Task<User> CreateAdminAsync(string name, string email, string? phone, string password)
		{
			var user = NewUser(RoleIds.Admin, name, email, phone, password);
			_context.Tbl_Users.Add(user);
			await _context.SaveChangesAsync();
			return user;
		}

		public async Task<StudentEntity?> GetOrCreateStudentProfileAsync(int userId)
		{
			var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
			if (student != null) return student;

			var user = await _context.Tbl_Users.FirstOrDefaultAsync(u => u.UserId == userId && u.RoleId == RoleIds.Student);
			if (user == null) return null;

			student = new StudentEntity
			{
				UserId = user.UserId,
				StudentID = await NewStudentCodeAsync(),
				Name = user.FullName ?? user.Email,
				Email = user.Email,
				Contact = user.MobileNo
			};
			_context.Students.Add(student);
			await _context.SaveChangesAsync();
			return student;
		}

		private async Task<string> NewStudentCodeAsync()
		{
			string code;
			do
			{
				code = $"ST{DateTime.Now:yyMM}{Random.Shared.Next(1000, 9999)}";
			} while (await _context.Students.AnyAsync(s => s.StudentID == code));
			return code;
		}

		private async Task<string> NewTeacherCodeAsync()
		{
			string code;
			do
			{
				code = $"T{DateTime.Now:yy}{Random.Shared.Next(1000, 9999)}";
			} while (await _context.Teachers.AnyAsync(t => t.TeacherID == code));
			return code;
		}
	}
}
