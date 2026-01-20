namespace MySummerCarMusicManager.Infrastructure.DataAccess;

public interface ISettingsSaver
{
    AppSettings Settings { get; }
    void Save();
    void Load();
}
