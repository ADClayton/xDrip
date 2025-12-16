using MauiGlucoseReceiver.Views;

namespace MauiGlucoseReceiver;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new MainPage());
    }
}
