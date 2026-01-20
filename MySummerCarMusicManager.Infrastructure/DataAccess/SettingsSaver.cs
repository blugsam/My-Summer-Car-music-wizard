using System.Text.Json;
using TagLib.Riff;

namespace MySummerCarMusicManager.Infrastructure.DataAccess;

internal sealed class SettingsSaver : ISettingsSaver
{
    private const string AppName = "MSC Music Wizard";
    private const string FileName = "settings.json";

    private readonly string _filePath;

    private readonly IFileSystem _fileSystem;

    public AppSettings Settings { get; private set; } = new();

    public SettingsSaver(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _filePath = GetAppDataPath();

        Load();
    }

    private static string GetAppDataPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, AppName);
        return Path.Combine(folder, FileName);
    }

    public void Load()
    {
        if (!_fileSystem.IsFileExists(_filePath))
        {
            Settings = new AppSettings();
            return;
        }

        try
        {
            var json = _fileSystem.ReadText(_filePath);
            Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            Settings = new AppSettings();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (dir != null && !_fileSystem.IsFolderExists(dir))
        {
            _fileSystem.CreateFolder(dir);
        }

        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });

        _fileSystem.WriteText(_filePath, json);
    }

}

public sealed record AppSettings
{
    public Dictionary<string, string> GamePaths { get; set; } = new();
}
