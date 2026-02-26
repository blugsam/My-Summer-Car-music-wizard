using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Classic.CommonControls;
using Classic.CommonControls.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySummerCarMusicManager.App.Music.Add;
using MySummerCarMusicManager.App.Music.Burn;
using MySummerCarMusicManager.App.Music.Get;
using MySummerCarMusicManager.App.Music.Move;
using MySummerCarMusicManager.App.Music.Remove;
using MySummerCarMusicManager.Domain;

namespace MySummerCarMusicManager.NET.ViewModels;

public partial class EditorViewModel : ViewModelBase
{
    public event Action? RequestChangeFolder;
    public event Action? RequestClose;

    private readonly GetMusic _getMusic;
    private readonly AddMusic _addMusic;
    private readonly MoveMusic _moveMusic;
    private readonly RemoveMusic _removeMusic;
    private readonly BurnMusic _syncMusic;

    [ObservableProperty]
    private string _gameFolderPath;

    public PlaylistCollection Tracks { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveTrackCommand))]
    private Music? _selectedTrack;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SyncCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddFilesCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSmallIcons))]
    [NotifyPropertyChangedFor(nameof(IsLargeIcons))]
    private ToolbarSize _toolbarSize = ToolbarSize.Large;

    public bool IsSmallIcons
    {
        get => ToolbarSize == ToolbarSize.Small;
        set { if (value) ToolbarSize = ToolbarSize.Small; }
    }

    public bool IsLargeIcons
    {
        get => ToolbarSize == ToolbarSize.Large;
        set { if (value) ToolbarSize = ToolbarSize.Large; }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTextDown))]
    [NotifyPropertyChangedFor(nameof(IsTextRight))]
    [NotifyPropertyChangedFor(nameof(IsTextNo))]
    private ToolbarTextPlacement _toolbarTextPlacement = ToolbarTextPlacement.Down;

    public bool IsTextDown
    {
        get => ToolbarTextPlacement == ToolbarTextPlacement.Down;
        set { if (value) ToolbarTextPlacement = ToolbarTextPlacement.Down; }
    }

    public bool IsTextRight
    {
        get => ToolbarTextPlacement == ToolbarTextPlacement.Right;
        set { if (value) ToolbarTextPlacement = ToolbarTextPlacement.Right; }
    }

    public bool IsTextNo
    {
        get => ToolbarTextPlacement == ToolbarTextPlacement.NoText;
        set { if (value) ToolbarTextPlacement = ToolbarTextPlacement.NoText; }
    }
    public EditorViewModel(
        string folderPath,
        GetMusic getMusic,
        AddMusic addMusic,
        MoveMusic moveMusic,
        RemoveMusic removeMusic,
        BurnMusic syncMusic)
    {
        _gameFolderPath = folderPath;
        _getMusic = getMusic;
        _addMusic = addMusic;
        _moveMusic = moveMusic;
        _removeMusic = removeMusic;
        _syncMusic = syncMusic;

        LoadLibraryCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadLibrary()
    {
        if (string.IsNullOrWhiteSpace(GameFolderPath)) return;

        IsBusy = true;
        StatusMessage = "Loading library...";
        Tracks.Clear();

        try
        {
            var loadedTracks = await _getMusic.Handle(GameFolderPath, CancellationToken.None);

            foreach (var track in loadedTracks)
            {
                Tracks.Add(track);
            }
            StatusMessage = $"{Tracks.Count} track(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanInteract))]
    private async Task AddFiles(object? parameter)
    {
        var topLevel = TopLevel.GetTopLevel(parameter as Control);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Add Music Files",
            AllowMultiple = true,
            FileTypeFilter = [new("Audio") { Patterns = ["*.mp3", "*.wav", "*.ogg", "*.flac", "*.m4a"] }]
        });

        if (files.Count == 0) return;

        IsBusy = true;
        StatusMessage = "Adding files...";

        try
        {
            var paths = files.Select(f => f.Path.LocalPath);
            int currentMax = Tracks.Count > 0 ? Tracks.Max(t => t.Position) : 0;

            var newTracks = await _addMusic.Handle(paths, currentMax, CancellationToken.None);

            foreach (var track in newTracks)
            {
                Tracks.Add(track);
            }

            StatusMessage = $"Added {newTracks.Count} tracks.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveTrack))]
    private void RemoveTrack(object? parameter)
    {
        if (parameter is System.Collections.IList items && items.Count > 0)
        {
            var idsToRemove = items.OfType<Music>().Select(m => m.Id).ToList();
            _removeMusic.Handle(Tracks, idsToRemove);
            StatusMessage = $"Removed {idsToRemove.Count} tracks.";
        }
        else if (SelectedTrack != null)
        {
            _removeMusic.Handle(Tracks, new[] { SelectedTrack.Id });
            StatusMessage = "Track removed.";
        }
    }

    public void MoveTrackItem(int oldIndex, int newIndex)
    {
        if (IsBusy) return;

        _moveMusic.Handle(Tracks, oldIndex, newIndex);
        StatusMessage = "Order changed. Sync required.";
    }

    [RelayCommand(CanExecute = nameof(CanInteract))]
    private async Task Sync()
    {
        if (Tracks.Count == 0) return;
        if (string.IsNullOrWhiteSpace(GameFolderPath))
        {
            StatusMessage = "Folder not selected!";
            return;
        }

        IsBusy = true;
        StatusMessage = "Burning CD... Please wait.";

        try
        {
            await _syncMusic.Handle(Tracks, GameFolderPath, CancellationToken.None);
            StatusMessage = "Sync completed successfully!";
            await LoadLibrary();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sync failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ChangeFolder() => RequestChangeFolder?.Invoke();

    [RelayCommand]
    private void CloseFolder() => RequestClose?.Invoke();

    [RelayCommand]
    private void SelectAll(DataGrid grid) => grid.SelectAll();

    [RelayCommand]
    private static async Task ShowAbout(Window parentWindow)
    {
        try
        {
            var uri = new Uri("avares://MySummerCarMusicManager.NET/Icons/internet_connection_wiz.png");
            using var bitmap = new Bitmap(AssetLoader.Open(uri));

            var options = new AboutDialogOptions
            {
                Title = "MSC Music Wizard",
                Copyright = "Developed by blugsam for MSC community (C) 2026",
                Icon = bitmap
            };

            await AboutDialog.ShowDialog(parentWindow, options);
        }
        catch
        {
            await AboutDialog.ShowDialog(parentWindow, new AboutDialogOptions { Title = "About" });
        }
    }

    private bool CanInteract() => !IsBusy;
    private bool CanRemoveTrack() => !IsBusy && SelectedTrack != null;
}
