using First.API.Models;
using GAMES.CORE.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace First.API.Services
{
    public class GameServices : IGamesServices
    {
        private readonly IMongoCollection<Games> _games;
        private readonly ILogger<GameServices> _logger;

        public GameServices(IOptions<GamesDBSettings> gamesDBSettings, IMongoClient mongoClient, ILogger<GameServices> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var gamesDatabase = mongoClient.GetDatabase(gamesDBSettings.Value.DatabaseName);
            _games = gamesDatabase.GetCollection<Games>(gamesDBSettings.Value.GamesCollectionName);
        }

        public List<Games> Get() =>
            _games.FindSync(game => true).ToList();

        public Games? Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Attempted to fetch game with empty or null Id.");
                return null;
            }

            try
            {
                if (!ObjectId.TryParse(id, out var objectId))
                {
                    _logger.LogWarning("Invalid ObjectId format provided: {Id}", id);
                    return null;
                }

                var cursor = _games.FindSync(g => g.Id == id);
                return cursor.FirstOrDefault();
            }
            //catch (MongoException ex)
            //{
            //    _logger.LogError(ex, "Failed to fetch game from database for Id {Id}", id);
            //    throw new ApplicationException("Failed to fetch game from database.", ex);
            //}
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch game from database for Id {Id}", id);
                throw new ApplicationException("Failed to fetch game from database.", ex);
            }

        }


        public Games Create(Games game)
        {
            _games.InsertOne(game);
            return game;
        }

        public void Update(string id, Games gameIn) =>
            _games.ReplaceOne(game => game.Id == id, gameIn);

        public void Remove(string id) =>
            _games.DeleteOne(game => game.Id == id);
    }
}
