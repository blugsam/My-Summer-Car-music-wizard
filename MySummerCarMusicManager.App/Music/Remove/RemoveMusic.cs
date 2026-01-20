using MySummerCarMusicManager.Domain;

namespace MySummerCarMusicManager.App.Music.Remove;

public sealed class RemoveMusic
{
    public void Handle(PlaylistCollection playlist, IEnumerable<Guid> trackIds)
    {
        var idsSet = trackIds.ToHashSet();

        var tracksToRemove = playlist
            .Where(t => idsSet.Contains(t.Id))
            .ToList();

        playlist.RemoveMany(tracksToRemove);
    }
}
