namespace GAMES.CORE.Models
{
    public interface IGamesDBSettings
    {
        string GamesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}