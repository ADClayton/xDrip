using Microsoft.Maui.Controls;

namespace MauiXdripListener;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
