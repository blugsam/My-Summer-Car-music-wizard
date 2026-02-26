using System;
using Microsoft.Extensions.DependencyInjection;
using MySummerCarMusicManager.NET.ViewModels;

namespace MySummerCarMusicManager.NET;

public static class DependencyInjection
{
    public static IServiceCollection AddView(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LauncherViewModel>();

        services.AddSingleton<Func<string, EditorViewModel>>(provider => (folderPath) => ActivatorUtilities.CreateInstance<EditorViewModel>(provider, folderPath));

        return services;
    }
}
