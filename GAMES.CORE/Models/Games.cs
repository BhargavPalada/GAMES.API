using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace First.API.Models
{
    public class Games
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("Author")]
        public string Author { get; set; } = string.Empty;

        [BsonElement("ReleaseDate")]
        public DateTime ReleaseDate { get; set; }

        [BsonElement("Genre")]
        public string Genre { get; set; } = string.Empty;
    }
}