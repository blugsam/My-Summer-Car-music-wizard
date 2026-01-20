using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MySummerCarMusicManager.NET.ViewModels;

namespace MySummerCarMusicManager.NET.Views;

public partial class LauncherView : UserControl
{
    public LauncherView()
    {
        InitializeComponent();
    }

    private void OnItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is LauncherViewModel vm)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (vm.NextStepCommand.CanExecute(topLevel))
            {
                vm.NextStepCommand.Execute(topLevel);
            }
        }
    }
}
