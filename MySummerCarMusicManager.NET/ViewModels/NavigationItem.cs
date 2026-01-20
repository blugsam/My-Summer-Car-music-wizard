using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MySummerCarMusicManager.NET.ViewModels;

public partial class NavigationItem : ObservableObject
{
    public object Model { get; }

    public string Text { get; }
    public Bitmap? SmallIcon { get; }
    public Bitmap? LargeIcon { get; }

    public NavigationItem(object model, string text, string iconName)
    {
        Model = model;
        Text = text;

        SmallIcon = new Bitmap(AssetLoader.Open(new Uri($"avares://MySummerCarMusicManager.NET/Icons/{iconName}-16.png")));
        LargeIcon = new Bitmap(AssetLoader.Open(new Uri($"avares://MySummerCarMusicManager.NET/Icons/{iconName}-32.png")));
    }
}
