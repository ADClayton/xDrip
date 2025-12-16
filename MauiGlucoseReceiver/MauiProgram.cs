using Microsoft.Extensions.Logging;

namespace MauiGlucoseReceiver;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Logging.AddDebug();

        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<Services.GlucoseBroadcastService>();
        builder.Services.AddTransient<ViewModels.MainPageViewModel>();
        builder.Services.AddTransient<Views.MainPage>();

        return builder.Build();
    }
}
