using GAMES.CORE.LoginDetails;
using GAMES.CORE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace GAMES.SERVICE.UserServices
{
    public class UserService : IUserService
    {
        // MongoDB collection instance for users
        private readonly IMongoCollection<UserModel> _usersCollection;

        // Constructor initializes MongoDB connection and collection, and sets up unique index on username
        public UserService(IOptions<AuthDBSettings> authDBSettings, IMongoClient mongoClient)
        {
            // Get the MongoDB database instance using database name from configuration
            var authDatabase = mongoClient.GetDatabase(authDBSettings.Value.DatabaseName);

            // Get the users collection from the database using configured collection name
            _usersCollection = authDatabase.GetCollection<UserModel>(authDBSettings.Value.UsersCollectionName);

            // Create a unique index on the Username field to enforce uniqueness of usernames
            var indexKeysDefinition = Builders<UserModel>.IndexKeys.Ascending(u => u.Username);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<UserModel>(indexKeysDefinition, indexOptions);
            _usersCollection.Indexes.CreateOne(indexModel);
        }

        // Authenticate a user by username and password
        // Returns the UserModel if authentication succeeds, or null if it fails
        public async Task<UserModel> AuthenticateUser(string username, string password)
        {
            // Find user document in the database matching the username
            var user = await _usersCollection
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync();

            // If user exists and password matches the stored hash, return user object
            if (user != null && VerifyPassword(password, user.Password))
            {
                return user;
            }

            // No matching user or password mismatch: return null
            return null!;
        }

        // Create a new user in the database
        public async Task<UserModel> CreateUser(UserModel user)
        {
            // Check if username already exists in database
            var existingUser = await GetUserByUsername(user.Username);
            if (existingUser != null)
                throw new Exception("Username already exists");

            // Hash the user's password before storing
            user.Password = HashPassword(user.Password);
            // Set the creation date to current UTC time
            user.CreatedAt = DateTime.UtcNow;
            // MongoDB will generate the ID automatically

            // Insert the new user document into the MongoDB collection asynchronously
            await _usersCollection.InsertOneAsync(user);
            return user;
        }

        // Retrieve user by their unique ID from MongoDB
        public async Task<UserModel> GetUserById(string id)
        {
            return await _usersCollection
                .Find(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        // Retrieve user by their username
        public async Task<UserModel> GetUserByUsername(string username)
        {
            return await _usersCollection
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        // Private utility method to hash password using SHA-256
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            // Convert hash bytes to a Base64 string for storage
            return Convert.ToBase64String(hash);
        }

        // Verify a plaintext input password against the stored password hash
        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            // Hash the input password and compare to stored hash
            var hashOfInput = HashPassword(inputPassword);
            return hashOfInput == storedHash;
        }
    }
}
