using MySummerCarMusicManager.Domain;

namespace MySummerCarMusicManager.App.Games
{
    public interface IGameCatalog
    {
        IEnumerable<Game> GetSupportedGames();
    }
}
