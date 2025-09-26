using GAMES.CORE.LoginDetails;
using GAMES.CORE.Models;

namespace GAMES.SERVICE.UserServices
{
    public interface IUserService
    {
        Task<UserModel> AuthenticateUser(string username, string password);
        Task<UserModel> CreateUser(UserModel user);
        Task<UserModel> GetUserById(string id);
        Task<UserModel> GetUserByUsername(string username);
    }
}