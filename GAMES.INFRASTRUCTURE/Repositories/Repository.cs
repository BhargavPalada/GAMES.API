using First.API.Models;
using GAMES.CORE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GAMES.INFRASTRUCTURE.Repositories
{
    public class Repository : IRepository
    {
        protected readonly IMongoCollection<Games> _collection;

        public Repository(IOptions<GamesDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _collection = database.GetCollection<Games>(nameof(Games));
        }

        
        public Repository(IMongoCollection<Games> collection)
        {
            _collection = collection;
        }

        public async Task<IEnumerable<Games>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<Games?> GetByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            return await _collection.Find(Builders<Games>.Filter.Eq("_id", objectId)).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Games entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Game entity cannot be null");
            }

            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, Games entity)
        {
            var objectId = new ObjectId(id);
            await _collection.ReplaceOneAsync(Builders<Games>.Filter.Eq("_id", objectId), entity);
        }

        public async Task DeleteAsync(string id)
        {
            var objectId = new ObjectId(id);
            await _collection.DeleteOneAsync(Builders<Games>.Filter.Eq("_id", objectId));
        }
    }
}
