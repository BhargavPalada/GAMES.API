using First.API.Models;
using First.API.Services;
using GAMES.CORE.Models;
using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;
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
            _gameServices = new TestableGameServices(optionsMock.Object, _mockClient.Object);
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

        // Optional: Mock insertion test
        [Fact]
        public void Create_ShouldInsertGame()
        {
            var newGame = new Games { Name = "New Game" };

            _gameServices.Create(newGame);

            _mockCollection.Verify(c => c.InsertOne(newGame, null, default), Times.Once);
        }

        // Subclass GameServices to allow injecting IMongoClient for easier testing
        private class TestableGameServices : GameServices
        {
            public TestableGameServices(IOptions<GamesDBSettings> settings, IMongoClient client)
                : base(settings)
            {
                var database = client.GetDatabase(settings.Value.DatabaseName);
                typeof(GameServices)
                    .GetField("_games", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(this, database.GetCollection<Games>(settings.Value.GamesCollectionName));
            }
        }
    }
}
