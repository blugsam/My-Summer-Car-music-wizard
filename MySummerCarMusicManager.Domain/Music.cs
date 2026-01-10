namespace MySummerCarMusicManager.Domain;

public sealed class Music
{
    public Guid Id { get; }
    public string Title { get; }
    public string[] Authors { get; }
    public string Path { get; }
    public ushort Position { get; }
    public bool IsConverted { get; }

    public Music(
        string title,
        string[] authors,
        string path,
        ushort position,
        bool isConverted)
    {
        Id = Guid.NewGuid();
        Title = title;
        Authors = authors;
        Path = path;
        Position = position;
        IsConverted = isConverted;
    }
}

