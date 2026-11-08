using EduLearn.Models;

namespace EduLearn.Services.Interfaces
{
    public interface IUserService
    {
        User GetUserByEmail(string email);
        bool VerifyPassword(string enteredPassword, string storedPassword, string salt);
        string CreateSaltKey(int size);
        string CreatePasswordHash(string password, string salt, string passwordFormat = "PBKDF2");
        bool ChangePassword(string email, string currentPassword, string newPassword);

    }
}
