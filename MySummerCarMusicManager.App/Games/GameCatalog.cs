using MySummerCarMusicManager.Domain;

namespace MySummerCarMusicManager.App.Games;

internal sealed class GameCatalog : IGameCatalog
{
    public IEnumerable<Game> GetSupportedGames()
    {
        yield return Game.CreateMySummerCar();
        yield return Game.CreateMyWinterCar();
    }
}
