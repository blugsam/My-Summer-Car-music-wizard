using CommunityToolkit.Mvvm.ComponentModel;

namespace MySummerCarMusicManager.Domain;

public sealed partial class Music : ObservableObject
{
    public Guid Id { get; }
    public string Title { get; init; }
    public string[] Authors { get; init; }
    public TimeSpan Duration { get; init; }
    public string Path { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRenamed))]
    [NotifyPropertyChangedFor(nameof(IsReady))]
    private int _position;
    public bool IsTranscoded => System.IO.Path.GetExtension(Path).Equals(".ogg", StringComparison.CurrentCultureIgnoreCase);
    public bool IsRenamed => System.IO.Path.GetFileNameWithoutExtension(Path).Equals($"Track{Position}", StringComparison.OrdinalIgnoreCase);
    public bool IsReady => IsTranscoded && IsRenamed;

    public Music(
        string title,
        string[] authors,
        TimeSpan duration,
        string path,
        int position)
    {
        Id = Guid.NewGuid();
        Title = title;
        Authors = authors;
        Duration = duration;
        Path = path;
        _position = position;
    }
}

