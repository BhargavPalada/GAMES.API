using First.API.Models;
using GAMES.CORE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace First.API.Services
{
    public class GameServices : IGamesServices
    {
        private readonly IMongoCollection<Games> _games;

        public GameServices(IOptions<GamesDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _games = database.GetCollection<Games>(settings.Value.GamesCollectionName);
        }

        public List<Games> Get() =>
            _games.Find(game => true).ToList();

        public Games Get(string id) =>
            _games.Find(game => game.Id == id).FirstOrDefault();

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
