namespace MySummerCarMusicManager.Domain;

public sealed class Music
{
    public Guid Id { get; }
    public string Title { get; init; }
    public string[] Authors { get; init; }
    public TimeSpan Duration { get; init; }
    public string Path { get; init; }
    public int Position { get; set; }
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
        Position = position;
    }
}

