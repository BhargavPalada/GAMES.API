namespace GAMES.CORE.Models
{
    public class LoginResponseModel
    {
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string Roles { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ExpiresIn { get; set; } // seconds
    }
}