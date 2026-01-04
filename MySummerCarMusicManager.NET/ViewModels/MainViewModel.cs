namespace MySummerCarMusicManager.NET.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public static string Greeting => "Welcome to Avalonia!";

    public static string RustText => RustInterop.GetMessage();
}
