using MauiGlucoseReceiver.ViewModels;

namespace MauiGlucoseReceiver.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext ??= new MainPageViewModel();
    }
}
