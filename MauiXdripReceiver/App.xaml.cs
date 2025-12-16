using Microsoft.Maui.Controls;

namespace MauiXdripReceiver;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
