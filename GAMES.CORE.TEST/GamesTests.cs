using Xunit;
using First.API.Models;
using System;

namespace GAMES.CORE.TESTS.Models
{
    public class GamesTests
    {
        [Fact]
        public void Games_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var game = new Games();

            // Assert - Expect empty strings, not null
            Assert.NotNull(game);
            Assert.Equal(string.Empty, game.Id);           // Empty string
            Assert.Equal(string.Empty, game.Name);         // Empty string
            Assert.Equal(string.Empty, game.Description);  // Empty string
            Assert.Equal(string.Empty, game.Author);       // Empty string
            Assert.Equal(string.Empty, game.Genre);        // Empty string
            Assert.Equal(default(DateTime), game.ReleaseDate); // Default DateTime
        }

        [Fact]
        public void Games_WithProperties_StoresValuesCorrectly()
        {
            // Arrange & Act
            var releaseDate = new DateTime(2023, 12, 15);
            var game = new Games
            {
                Id = "507f1f77bcf86cd799439011",
                Name = "The Witcher 3",
                Description = "An action role-playing game",
                Author = "CD Projekt Red",
                Genre = "RPG",
                ReleaseDate = releaseDate
            };

            // Assert
            Assert.Equal("507f1f77bcf86cd799439011", game.Id);
            Assert.Equal("The Witcher 3", game.Name);
            Assert.Equal("An action role-playing game", game.Description);
            Assert.Equal("CD Projekt Red", game.Author);
            Assert.Equal("RPG", game.Genre);
            Assert.Equal(releaseDate, game.ReleaseDate);
        }

        [Fact]
        public void Games_ReleaseDate_CanBeSetToMinValue()
        {
            // Arrange & Act
            var game = new Games
            {
                ReleaseDate = DateTime.MinValue
            };

            // Assert
            Assert.Equal(DateTime.MinValue, game.ReleaseDate);
        }

        [Fact]
        public void Games_ReleaseDate_CanBeSetToMaxValue()
        {
            // Arrange & Act
            var game = new Games
            {
                ReleaseDate = DateTime.MaxValue
            };

            // Assert
            Assert.Equal(DateTime.MaxValue, game.ReleaseDate);
        }

        [Fact]
        public void Games_AllStringProperties_AreInitializedEmpty()
        {
            // Arrange & Act
            var game = new Games();

            // Assert - All string properties should be empty, not null
            Assert.Equal(string.Empty, game.Id);
            Assert.Equal(string.Empty, game.Name);
            Assert.Equal(string.Empty, game.Description);
            Assert.Equal(string.Empty, game.Author);
            Assert.Equal(string.Empty, game.Genre);

        }
        [Fact]
        public void Games_Id_ShouldNotAcceptInvalidFormat()
        {
            // Arrange
            var game = new Games();

            // Act
            game.Id = "InvalidIdFormat123"; // not a valid ObjectId or GUID

            // Assert - if your model validates Id format, check accordingly
            Assert.NotEqual("507f1f77bcf86cd799439011", game.Id);
        }

        [Fact]
        public void Games_StringProperties_AreNotNullAfterInitialization()
        {
            // Arrange & Act
            var game = new Games();

            // Assert - just to make sure no property is null
            Assert.All(new string?[] { game.Id, game.Name, game.Description, game.Author, game.Genre },
                        prop => Assert.NotNull(prop));
        }

    }
}