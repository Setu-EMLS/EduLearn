using EduLearn.Models;

namespace EduLearn.Services.Interfaces
{
    public interface IIdentityService
    {
        Task signInUser(User user);
    }
}
