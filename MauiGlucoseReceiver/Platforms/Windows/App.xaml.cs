using Microsoft.UI.Xaml;

namespace MauiGlucoseReceiver.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiGlucoseReceiver.MauiProgram.CreateMauiApp();
}
