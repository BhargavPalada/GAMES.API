using First.API.Models;
using GAMES.CORE.Models;
using GAMES.INFRASTRUCTURE.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GAMES.INFRASTRUCTURE.Tests
{
    public class RepositoryTests
    {
        private readonly Mock<IMongoCollection<Games>> _mockCollection;
        private readonly Repository _repository;

        public RepositoryTests()
        {
            _mockCollection = new Mock<IMongoCollection<Games>>();
            _repository = new Repository(_mockCollection.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Call_InsertOneAsync()
        {
            var game = new Games { Id = ObjectId.GenerateNewId().ToString(), Name = "Test Game" };

            await _repository.CreateAsync(game);

            _mockCollection.Verify(c => c.InsertOneAsync(game, null, default), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Call_ReplaceOneAsync()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var game = new Games { Id = id, Name = "Updated Game" };

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Games>>(),
                game,
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, ObjectId.GenerateNewId()));

            await _repository.UpdateAsync(id, game);

            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Games>>(),
                game,
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Call_DeleteOneAsync()
        {
            var id = ObjectId.GenerateNewId().ToString();

            _mockCollection.Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<Games>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            await _repository.DeleteAsync(id);

            _mockCollection.Verify(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<Games>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Call_Find()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var expectedGame = new Games { Id = id, Name = "Mock Game" };

            // Mock IAsyncCursor with proper async cursor behavior
            var mockCursor = new Mock<IAsyncCursor<Games>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<Games> { expectedGame });

            // Setup FindAsync to return the mocked cursor
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Games>>(),
                It.IsAny<FindOptions<Games, Games>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedGame.Id, result.Id);
            Assert.Equal(expectedGame.Name, result.Name);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_If_Null_Game()
        {
            await Assert.ThrowsAsync<System.ArgumentNullException>(() => _repository.CreateAsync((Games)null!));
        }

        [Fact]
        public async Task UpdateAsync_Should_Handle_NonExistent_Game()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var game = new Games { Id = id, Name = "NonExistent" };

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Games>>(),
                game,
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(0, 0, ObjectId.GenerateNewId())); // Simulate no match

            await _repository.UpdateAsync(id, game);

            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Games>>(),
                game,
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }



        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent_Id()
        {
            var id = ObjectId.GenerateNewId().ToString();

            var mockCursor = new Mock<IAsyncCursor<Games>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false); // No data
            mockCursor.SetupGet(c => c.Current).Returns(new List<Games>());

            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Games>>(),
                It.IsAny<FindOptions<Games, Games>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            var result = await _repository.GetByIdAsync(id);

            Assert.Null(result);
        }

    }
}
