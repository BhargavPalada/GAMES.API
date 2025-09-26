using First.API.Models;
using First.API.Services;
using GAMES.CORE.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GAMES.SERVICE.TESTS.Services
{
    public class GameServicesTests
    {
        private readonly Mock<IMongoClient> _mockClient;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<Games>> _mockCollection;
        private readonly GameServices _gameServices;
        private readonly Mock<ILogger<GameServices>> _mockLogger;


        public GameServicesTests()
        {
            var dbSettings = new GamesDBSettings
            {
                ConnectionString = "mongodb://localhost:27017", // Dummy string for test, won't connect
                DatabaseName = "GameDB",
                GamesCollectionName = "Games"
            };
            var optionsMock = new Mock<IOptions<GamesDBSettings>>();
            optionsMock.Setup(x => x.Value).Returns(dbSettings);

            _mockCollection = new Mock<IMongoCollection<Games>>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockClient = new Mock<IMongoClient>();

            // Setup hierarchy for MongoClient -> Database -> Collection
            _mockClient.Setup(c => c.GetDatabase(dbSettings.DatabaseName, null))
                .Returns(_mockDatabase.Object);
            _mockDatabase.Setup(d => d.GetCollection<Games>(dbSettings.GamesCollectionName, null))
                .Returns(_mockCollection.Object);

            // Inject the mock client into a subclass of GameServices (that accepts IMongoClient)
            _mockLogger = new Mock<ILogger<GameServices>>();

            _gameServices = new TestableGameServices(optionsMock.Object, _mockClient.Object, _mockLogger.Object);


        }

        [Fact]
        public void Get_ShouldReturnListOfGames()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Games>>();
            var games = new List<Games>
            {
                new Games { Name = "Game 1" },
                new Games { Name = "Game 2" }
            };
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupGet(_ => _.Current).Returns(games);
            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<Games>>(),
                It.IsAny<FindOptions<Games, Games>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            // Act
            var result = _gameServices.Get();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Collection(result,
                item => Assert.Equal("Game 1", item.Name),
                item => Assert.Equal("Game 2", item.Name));
        }

        [Fact]
        public void Get_ById_ShouldReturnGame()
        {
            // Arrange
            var expectedGame = new Games { Id = "64c13ab08edf48a008793cac", Name = "Test Game" };
            var mockCursor = new Mock<IAsyncCursor<Games>>();
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupGet(_ => _.Current).Returns(new List<Games> { expectedGame });
            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<Games>>(),
                It.IsAny<FindOptions<Games, Games>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            // Act
            var result = _gameServices.Get("64c13ab08edf48a008793cac");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedGame.Name, result.Name);
        }

        
        [Fact]
        public void Create_ShouldInsertGame()
        {
            var newGame = new Games { Name = "New Game" };

            _gameServices.Create(newGame);

            _mockCollection.Verify(c => c.InsertOne(newGame, null, default), Times.Once);
        }

        [Fact]
        public void Get_ShouldReturnEmptyList_WhenNoGamesExist()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Games>>();
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                      .Returns(false); // No items
            mockCursor.SetupGet(_ => _.Current).Returns(new List<Games>());

            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<Games>>(),
                It.IsAny<FindOptions<Games, Games>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            // Act
            var result = _gameServices.Get();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Get_ById_ShouldReturnNull_WhenGameNotFound()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Games>>();
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                      .Returns(false); // No matching record
            mockCursor.SetupGet(_ => _.Current).Returns(new List<Games>());

            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<Games>>(),
                It.IsAny<FindOptions<Games, Games>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            // Act
            var result = _gameServices.Get("non_existing_id");

            // Assert
            Assert.Null(result);
        }



        // Subclass GameServices to allow injecting IMongoClient for easier testing
        private class TestableGameServices : GameServices
        {
            public TestableGameServices(IOptions<GamesDBSettings> settings, IMongoClient client, ILogger<GameServices> logger)
                : base(settings, client, logger)
            {
                var database = client.GetDatabase(settings.Value.DatabaseName);
                typeof(GameServices)
                    .GetField("_games", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(this, database.GetCollection<Games>(settings.Value.GamesCollectionName));
            }
        }

        [Fact]
        public void Get_ShouldThrow_WhenMongoFindFails()
        {
            // Arrange
            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<Games>>(),
                It.IsAny<FindOptions<Games, Games>>(),
                It.IsAny<CancellationToken>()))
                .Throws(new MongoException("Database error"));

            // Act & Assert
            Assert.Throws<MongoException>(() => _gameServices.Get());
        }

        [Fact]
        public void Create_ShouldThrow_WhenInsertFails()
        {
            // Arrange
            var newGame = new Games { Name = "Broken Game" };
            _mockCollection.Setup(c => c.InsertOne(newGame, null, default))
                           .Throws(new MongoException("Insert failed"));

            // Act & Assert
            Assert.Throws<MongoException>(() => _gameServices.Create(newGame));
        }


        [Fact]
        public void Get_ById_ShouldReturnNull_WhenIdFormatIsInvalid()
        {
            var result = _gameServices.Get("invalid_format_id");
            Assert.Null(result);
        }

        [Fact]
        public void Get_ShouldLogWarning_WhenIdIsNullOrEmpty()
        {
            // Act
            var result = _gameServices.Get(string.Empty);

            // Assert
            Assert.Null(result);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("empty or null Id")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        
        [Fact]
        public void Get_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange - simulate exception when Mongo is called
            _mockCollection.Setup(c => c.FindSync(
     It.IsAny<FilterDefinition<Games>>(),
     It.IsAny<FindOptions<Games, Games>>(),
     It.IsAny<CancellationToken>()))
     .Throws(new MongoException("Mongo failure"));


            // Act & Assert
            var ex = Assert.Throws<ApplicationException>(() => _gameServices.Get("507f1f77bcf86cd799439011"));
            Assert.IsType<MongoException>(ex.InnerException);
            Assert.Equal("Mongo failure", ex.InnerException!.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch game from database")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

    }
}
