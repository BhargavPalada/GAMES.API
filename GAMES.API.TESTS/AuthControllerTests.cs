using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GAMES.CORE.LoginDetails;
using GAMES.CORE.Models;
using GAMES.SERVICE.JWTServices;
using GAMES.SERVICE.UserServices;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using First.API.Controllers;

namespace GAMES.API.TESTS
{
    public class AuthControllerTests
    {
        private readonly Mock<IJWTService> _mockJwtService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockJwtService = new Mock<IJWTService>();
            _mockUserService = new Mock<IUserService>();
            _controller = new AuthController(_mockJwtService.Object, _mockUserService.Object);
        }

        // ----------- Login Tests -----------

        [Fact]
        public async Task Login_Positive_ReturnsOkWithToken()
        {
            var login = new LoginRequestModel { UserName = "user", Password = "pass" };
            var user = new UserModel { Username = "user", Password = "hashed", Role = "User", Email = "user@email.com" };
            var token = "jwt-token";
            var response = new LoginResponseModel { UserName = "user", AccessToken = token, Role = "User", ExpiresAt = DateTime.UtcNow.AddHours(1), ExpiresIn = 3600 };

            _mockUserService.Setup(s => s.AuthenticateUser(login.UserName, login.Password)).ReturnsAsync(user);
            _mockJwtService.Setup(s => s.GenerateToken(user)).Returns(token);
            _mockJwtService.Setup(s => s.CreateLoginResponse(user, token)).Returns(response);

            var result = await _controller.Login(login);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task Login_Negative_InvalidCredentials_ReturnsUnauthorized()
        {
            var login = new LoginRequestModel { UserName = "user", Password = "wrong" };
            _mockUserService.Setup(s => s.AuthenticateUser(login.UserName!, login.Password!)).ReturnsAsync((UserModel?)null);

            var result = await _controller.Login(login);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_Neutral_MissingFields_ReturnsBadRequest()
        {
            var login = new LoginRequestModel { UserName = "", Password = "" };

            var result = await _controller.Login(login);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username and password are required", badRequest.Value);
        }

        // ----------- Register Tests -----------

        [Fact]
        public async Task Register_Positive_ReturnsOkWithUser()
        {
            var userModel = new UserModel { Username = "newuser", Password = "pass", Role = "User", Email = "new@email.com" };
            var createdUser = new UserModel { Username = "newuser", Password = "", Role = "User", Email = "new@email.com" };

            _mockUserService.Setup(s => s.CreateUser(userModel)).ReturnsAsync(createdUser);

            var result = await _controller.Register(userModel);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("User registered successfully", okResult.Value?.ToString());
        }

        [Fact]
        public async Task Register_Negative_UsernameExists_ReturnsBadRequest()
        {
            var userModel = new UserModel { Username = "existing", Password = "pass" };
            _mockUserService.Setup(s => s.CreateUser(userModel)).ThrowsAsync(new Exception("Username already exists"));

            var result = await _controller.Register(userModel);

            var badRequest = Assert.IsType<BadRequestObjectResult?>(result);
            Assert.Contains("Username already exists", badRequest?.Value?.ToString());
        }

        [Fact]
        public async Task Register_Neutral_MissingFields_ReturnsBadRequest()
        {
            var userModel = new UserModel { Username = "", Password = "" };

            var result = await _controller.Register(userModel);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username and password are required", badRequest.Value);
        }

        // ----------- ValidateToken Tests -----------

        [Fact]
        public void ValidateToken_Positive_ValidToken_ReturnsOk()
        {
            // Arrange
            var tokenModel = new TokenModel { Token = "valid-token" };
            _mockJwtService.Setup(s => s.ValidateToken(tokenModel.Token)).Returns(new ClaimsPrincipal());

            // Act
            var result = _controller.ValidateToken(tokenModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TokenValidationResponse>(okResult.Value);
            Assert.True(response.Valid);
        }

        [Fact]
        public void ValidateToken_Negative_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var tokenModel = new TokenModel { Token = "invalid-token" };
            _mockJwtService.Setup(s => s.ValidateToken(tokenModel.Token)).Throws(new Exception("Invalid token"));

            // Act
            var result = _controller.ValidateToken(tokenModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<TokenValidationResponse>(badRequestResult.Value);
            Assert.False(response.Valid);
        }

        // ----------- GetProfile Tests -----------

        [Fact]
        public async Task GetProfile_Positive_ReturnsOkWithUser()
        {
            var username = "user";
            var user = new UserModel { Username = username, Password = "pass", Role = "User", Email = "user@email.com" };
            _mockUserService.Setup(s => s.GetUserByUsername(username)).ReturnsAsync(user);

            var controller = _controller;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }));

            var result = await controller.GetProfile();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserModel>(okResult.Value);
            Assert.Equal(username, returnedUser.Username);
            Assert.Null(returnedUser.Password);
        }

        [Fact]
        public async Task GetProfile_Negative_UserNotFound_ReturnsNotFound()
        {
            var username = "nouser";

            // Cast the null literal to the nullable UserModel? type
            _mockUserService?.Setup(s => s.GetUserByUsername(username)).ReturnsAsync((UserModel?)null);

            var controller = _controller;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }));

            var result = await controller.GetProfile();

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
