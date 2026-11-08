using EduLearn.Models;

namespace EduLearn.Services.Interfaces
{
	public interface IAccountService
	{
		bool EmailExists(string email, int? exceptUserId = null);

		/// <summary>Creates a login (role Student) together with its Student profile.</summary>
		Task<StudentEntity> CreateStudentAsync(string name, string email, string? phone, string password);

		/// <summary>Creates a login (role Teacher) together with its Teacher profile.</summary>
		Task<TeacherEntity> CreateTeacherAsync(string name, string email, string? phone, string password, string? designation = null);

		Task<User> CreateAdminAsync(string name, string email, string? phone, string password);

		void SetPassword(User user, string password);

		/// <summary>Returns the Student profile for a user, creating it from the login details if it is missing.</summary>
		Task<StudentEntity?> GetOrCreateStudentProfileAsync(int userId);
	}
}
