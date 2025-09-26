using GAMES.CORE.LoginDetails;
using GAMES.CORE.Models;
using System.Security.Claims;

namespace GAMES.SERVICE.JWTServices
{
    public interface IJWTService
    {
        string GenerateToken(UserModel user);
        ClaimsPrincipal ValidateToken(string token);
        LoginResponseModel CreateLoginResponse(UserModel user, string token);
    }
}