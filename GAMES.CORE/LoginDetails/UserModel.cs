using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace GAMES.CORE.LoginDetails
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username can't be longer than 50 characters")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("Password")]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Password can't be longer than 255 characters")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("Role")]
        [Required(ErrorMessage = "Role is required")]
        [StringLength(20, ErrorMessage = "Role can't be longer than 20 characters")]
        public string Role { get; set; } = "User";   // default User role

        [BsonElement("Email")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("CreatedAt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
