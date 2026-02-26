using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MySummerCarMusicManager.Infrastructure.DataAccess;

namespace MySummerCarMusicManager.NET.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    private readonly LauncherViewModel _launcherViewModel;
    private readonly Func<string, EditorViewModel> _editorFactory;
    private readonly ISettingsSaver _settingsSaver;

    public MainViewModel(
        LauncherViewModel launcherViewModel,
        Func<string, EditorViewModel> editorFactory,
        ISettingsSaver settingsSaver)
    {
        _launcherViewModel = launcherViewModel;
        _editorFactory = editorFactory;
        _settingsSaver = settingsSaver;

        _currentView = _launcherViewModel;

        InitializeLauncher();
    }

    private void InitializeLauncher()
    {
        _launcherViewModel.SetKnownPaths(_settingsSaver.Settings.GamePaths);

        _launcherViewModel.OnGamePathFound += HandleGamePathFound;
        _launcherViewModel.OnContextReady += NavigateToEditor;
    }

    private void HandleGamePathFound(string gameId, string path)
    {
        _settingsSaver.Settings.GamePaths[gameId] = path;
        _settingsSaver.Save();
    }

    private void NavigateToEditor(string fullPath)
    {
        if (!System.IO.Directory.Exists(fullPath))
        {
            return;
        }

        var editorVm = _editorFactory(fullPath);

        void OnEditorCloseRequested()
        {
            editorVm.RequestClose -= OnEditorCloseRequested;

            NavigateToLauncher();
        }

        editorVm.RequestClose += OnEditorCloseRequested;

        CurrentView = editorVm;
    }

    private void NavigateToLauncher()
    {
        _launcherViewModel.Reset();
        CurrentView = _launcherViewModel;
    }
}
