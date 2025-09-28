using GAMES.CORE.LoginDetails;
using GAMES.CORE.Models;
using GAMES.SERVICE.JWTServices;
using GAMES.SERVICE.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace First.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly IJWTService _jwtService;
        private readonly IUserService _userService;

        public AuthController(IJWTService jwtService, IUserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel login)
        {
            try
            {
                if (string.IsNullOrEmpty(login.UserName) || string.IsNullOrEmpty(login.Password))
                    return BadRequest("Username and password are required");

                var user = await _userService.AuthenticateUser(login.UserName, login.Password);

                if (user == null)
                    return Unauthorized("Invalid credentials");

                var token = _jwtService.GenerateToken(user);
                var response = _jwtService.CreateLoginResponse(user, token);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            try
            {
                if (string.IsNullOrEmpty(userModel.Username) || string.IsNullOrEmpty(userModel.Password))
                    return BadRequest("Username and password are required");

                var createdUser = await _userService.CreateUser(userModel);

                return Ok(new
                {
                    message = "User registered successfully",
                    user = createdUser
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Registration failed: {ex.Message}");
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public IActionResult ValidateToken([FromBody] TokenModel tokenModel)
        {
            try
            {
                if (string.IsNullOrEmpty(tokenModel?.Token))
                    return BadRequest(new TokenValidationResponse { Valid = false, Message = "Token is required" });

                var principal = _jwtService.ValidateToken(tokenModel.Token);
                return Ok(new TokenValidationResponse { Valid = true, Message = "Token is valid" });
            }
            catch (Exception ex)
            {
                return BadRequest(new TokenValidationResponse { Valid = false, Message = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize(Roles ="Admin,Readonly,Moderator")]
        public async Task<IActionResult> GetProfile()
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User identity not found");

            var user = await _userService.GetUserByUsername(username);

            if (user == null)
                return NotFound("User not found");

            // Remove sensitive data
            user.Password = null;

            return Ok(user);
        }
    }

    public class TokenModel
    {   
        public string? Token { get; set; }
    }
}