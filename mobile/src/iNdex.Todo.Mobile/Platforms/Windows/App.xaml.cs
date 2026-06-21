using Microsoft.UI.Xaml;

namespace iNdex.Todo.Mobile.Platforms.Windows;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
