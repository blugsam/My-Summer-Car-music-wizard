using Microsoft.Extensions.DependencyInjection;
using MySummerCarMusicManager.App.Games;
using MySummerCarMusicManager.App.Music.Add;
using MySummerCarMusicManager.App.Music.Burn;
using MySummerCarMusicManager.App.Music.Get;
using MySummerCarMusicManager.App.Music.Move;
using MySummerCarMusicManager.App.Music.Remove;

namespace MySummerCarMusicManager.App;

public static class DependencyInjection
{
    public static IServiceCollection AddApp(this IServiceCollection services)
    {
        services.AddTransient<GetMusic>();
        services.AddTransient<AddMusic>();
        services.AddTransient<MoveMusic>();
        services.AddTransient<RemoveMusic>();
        services.AddTransient<BurnMusic>();

        services.AddSingleton<IGameCatalog, GameCatalog>();

        return services;
    }
}
