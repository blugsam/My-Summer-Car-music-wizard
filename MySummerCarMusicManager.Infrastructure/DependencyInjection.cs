using Microsoft.Extensions.DependencyInjection;
using MySummerCarMusicManager.Infrastructure.DataAccess;
using MySummerCarMusicManager.Infrastructure.Interop;

namespace MySummerCarMusicManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IMetadataReader, MetadataReader>();
        services.AddSingleton<ISettingsSaver, SettingsSaver>();
        services.AddSingleton<IRustInteropHandler, RustInteropHandler>();
        services.AddSingleton<IAudioTranscoder, AudioTranscoder>();

        return services;
    }
}
