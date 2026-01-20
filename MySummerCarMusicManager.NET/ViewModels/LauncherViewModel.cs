using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySummerCarMusicManager.App.Games;
using MySummerCarMusicManager.Domain;
using MySummerCarMusicManager.Infrastructure.DataAccess;

namespace MySummerCarMusicManager.NET.ViewModels;

public partial class LauncherViewModel : ViewModelBase
{
    private readonly IGameCatalog _gameCatalog;
    private readonly IFileSystem _fileSystem;

    public event Action<string>? OnContextReady;
    public event Action<string, string>? OnGamePathFound;

    private Dictionary<string, string> _knownPaths = [];

    [ObservableProperty]
    private string _title = "Select Game";

    [ObservableProperty]
    private bool _isGameSelectionStep = true;

    public ObservableCollection<IDisplayItem> Items { get; } = [];

    [ObservableProperty]
    private IDisplayItem? _selectedItem;

    private Game? _selectedGame;
    private string _currentRootPath = string.Empty;

    public void Initialize() => ShowGameSelection();

    public LauncherViewModel(
        IGameCatalog gameCatalog,
        IFileSystem fileSystem)
    {
        _gameCatalog = gameCatalog;
        _fileSystem = fileSystem;

        ShowGameSelection();
    }

    public void SetKnownPaths(Dictionary<string, string> paths)
    {
        _knownPaths = paths;
    }

    public void Reset()
    {
        _selectedGame = null;
        _currentRootPath = string.Empty;
        ShowGameSelection();
    }

    private void ShowGameSelection()
    {
        IsGameSelectionStep = true;
        Title = "Select Game";
        Items.Clear();

        foreach (var game in _gameCatalog.GetSupportedGames())
        {
            Items.Add(game);
        }
    }

    private void ShowSourceSelection(Game game, string rootPath)
    {
        _selectedGame = game;
        _currentRootPath = rootPath;

        IsGameSelectionStep = false;
        Title = $"{game.Name} Source";
        Items.Clear();

        foreach (var playlist in game.Playlists)
        {
            Items.Add(playlist);
        }
    }


    [RelayCommand]
    private async Task NextStep(object? parameter)
    {
        await (SelectedItem switch
        {
            Game game => OnGameSelectedAsync(game, parameter),
            Playlist playlist => OnPlaylistSelectedAsync(playlist),
            _ => Task.CompletedTask
        });
    }

    private async Task OnGameSelectedAsync(Game game, object? parameter)
    {
        if (TryGetCachedPath(game.Id, out var path))
        {
            ShowSourceSelection(game, path);
            return;
        }

        var pickedPath = await PickGameFolderAsync(game.Name, parameter);

        if (!string.IsNullOrEmpty(pickedPath))
        {
            OnGamePathFound?.Invoke(game.Id, pickedPath);

            _knownPaths[game.Id] = pickedPath;

            ShowSourceSelection(game, pickedPath);
        }
    }

    private Task OnPlaylistSelectedAsync(Playlist playlist)
    {
        var fullPath = Path.Combine(_currentRootPath, playlist.RelativePath);

        if (!_fileSystem.IsFolderExists(fullPath))
        {
            _fileSystem.CreateFolder(fullPath);
        }

        OnContextReady?.Invoke(fullPath);

        return Task.CompletedTask;
    }

    private bool TryGetCachedPath(string gameId, out string path)
    {
        if (_knownPaths.TryGetValue(gameId, out var cachedPath) && _fileSystem.IsFolderExists(cachedPath))
        {
            path = cachedPath;
            return true;
        }

        path = string.Empty;
        return false;
    }

    private static async Task<string?> PickGameFolderAsync(string gameName, object? parameter)
    {
        var topLevel = TopLevel.GetTopLevel(parameter as Control);
        if (topLevel == null) return null;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = $"Select {gameName} Installation Folder",
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    [RelayCommand]
    private void GoBack()
    {
        if (!IsGameSelectionStep)
        {
            ShowGameSelection();
        }
    }
}
