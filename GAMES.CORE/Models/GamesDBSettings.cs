namespace GAMES.CORE.Models
{
    public class GamesDBSettings : IGamesDBSettings
    {
        public string GamesCollectionName { get; set; } = null!;
        public string ConnectionString { get; set; } = "Games";
        public string DatabaseName { get; set; } = null!;
    }

    
}