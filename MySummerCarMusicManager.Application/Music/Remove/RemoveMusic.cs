using MySummerCarMusicManager.Domain;

namespace MySummerCarMusicManager.Application.Music.Remove;

public static class RemoveMusic
{
    public static void Handle(PlaylistCollection playlist, Guid trackId)
    {
        var trackToRemove = playlist.FirstOrDefault(t => t.Id == trackId);

        if (trackToRemove is null)
        {
            return;
        }

        playlist.Remove(trackToRemove);
    }
}
