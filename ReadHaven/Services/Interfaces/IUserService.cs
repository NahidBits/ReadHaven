using ReadHaven.Models.User;
using System.Threading.Tasks;

namespace ReadHaven.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> VerifyPasswordAsync(User user, string password);
        Task SignInAsync(User user);
    }
}
