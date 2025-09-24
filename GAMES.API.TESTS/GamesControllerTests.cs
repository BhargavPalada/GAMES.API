using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using First.API.Controllers;
using First.API.Models;
using First.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace First.API.Tests
{
    public class GamesControllerTests
    {
        private readonly Mock<ILogger<GamesController>> _mockLogger;
        //private readonly Mock<GameServices> _mockGameServices;
        private readonly GamesController _controller;
        private readonly Mock<IGamesServices> _mockGameServices;

        public GamesControllerTests()
        {
            _mockLogger = new Mock<ILogger<GamesController>>();
            _mockGameServices = new Mock<IGamesServices>();  // mock interface, NOT class
            _controller = new GamesController(_mockLogger.Object,_mockGameServices.Object);
        }


        
        [Fact]
        public void Get_ReturnsListOfGames()
        {
            var gamesList = new List<Games> {
        new Games { Id = "1", Name = "Game1" },
        new Games { Id = "2", Name = "Game2" }
    };
            _mockGameServices.Setup(service => service.Get()).Returns(gamesList);

            var result = _controller.Get();

            var actionResult = Assert.IsType<ActionResult<List<Games>>>(result);
            var returnValue = Assert.IsType<List<Games>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);

            _mockLogger.Verify(logger =>
                logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => state.ToString().Contains("Fetching all games")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }


        [Fact]
        public void Get_WithValidId_ReturnsGame()
        {
            var game = new Games { Id = "1", Name = "Game1" };
            _mockGameServices.Setup(service => service.Get("1")).Returns(game);

            var result = _controller.Get("1");

            var actionResult = Assert.IsType<ActionResult<Games>>(result);
            var returnValue = Assert.IsType<Games>(actionResult.Value);
            Assert.Equal("1", returnValue.Id);
        }

        [Fact]
        public void Get_WithInvalidId_ReturnsNotFound()
        {
            _mockGameServices.Setup(service => service.Get("100")).Returns((Games)null);

            var result = _controller.Get("100");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Create_ValidGame_ReturnsCreatedAtRoute()
        {
            var game = new Games { Id = "1", Name = "New Game" };

            var result = _controller.Create(game);

            var actionResult = Assert.IsType<ActionResult<Games>>(result);
            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(actionResult.Result);
            Assert.Equal("GetGame", createdAtRouteResult.RouteName);
            Assert.Equal(game, createdAtRouteResult.Value);
        }

        [Fact]
        public void Update_ExistingGame_ReturnsNoContent()
        {
            var game = new Games { Id = "1", Name = "Updated Game" };
            _mockGameServices.Setup(service => service.Get("1")).Returns(game);

            var result = _controller.Update("1", game);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Update_NonExistingGame_ReturnsNotFound()
        {
            _mockGameServices.Setup(service => service.Get("100")).Returns((Games)null);

            var result = _controller.Update("100", new Games());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_ExistingGame_ReturnsNoContent()
        {
            var game = new Games { Id = "1", Name = "Game to Delete" };
            _mockGameServices.Setup(service => service.Get("1")).Returns(game);

            var result = _controller.Delete("1");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_NonExistingGame_ReturnsNotFound()
        {
            _mockGameServices.Setup(service => service.Get("100")).Returns((Games)null);

            var result = _controller.Delete("100");

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
