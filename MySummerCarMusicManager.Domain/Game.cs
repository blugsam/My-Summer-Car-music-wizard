namespace MySummerCarMusicManager.Domain;

public sealed record Game : IDisplayItem
{
    public string Id { get; }
    public string Name { get; }
    public string IconName { get; }
    public IReadOnlyCollection<Playlist> Playlists { get; }

    private Game(string id, string name, string iconName, List<Playlist> playlists)
    {
        Id = id;
        Name = name;
        IconName = iconName;
        Playlists = playlists;
    }

    public static Game CreateMySummerCar()
    {
        var playlists = new List<Playlist>
        {
            new("Radio ", "Radio", "radio"),
            new("CD1",   "CD1",   "cd"),
            new("CD2",   "CD2",   "cd"),
            new("CD3",   "CD3",   "cd")
        };

        return new Game("MSC", "My Summer Car", "msc", playlists);
    }

    public static Game CreateMyWinterCar()
    {
        var playlists = new List<Playlist>
        {
            new("Radio ", "Radio", "radio"),
            new("CD1",   "CD1",   "cd"),
            new("CD2",   "CD2",   "cd"),
            new("CD3",   "CD3",   "cd")
        };

        return new Game("MWC", "My Winter Car", "mwc", playlists);
    }
}
