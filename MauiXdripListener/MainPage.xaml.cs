using MauiXdripListener.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace MauiXdripListener;

public partial class MainPage : ContentPage
{
    private readonly IGlucoseBroadcastService broadcastService;
    private bool initialized;

    public MainPage(IGlucoseBroadcastService broadcastService)
    {
        InitializeComponent();
        this.broadcastService = broadcastService;
        Appearing += OnAppearing;
        Disappearing += OnDisappearing;
    }

    private async void OnAppearing(object? sender, EventArgs e)
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        broadcastService.BroadcastReceived += OnBroadcastReceived;
        StatusLabel.Text = "Requesting latest broadcast from xDrip...";

        var reading = await broadcastService.TryGetLastBroadcastAsync();
        if (reading != null)
        {
            UpdateUi(reading, fromStartup: true);
        }
        else
        {
            StatusLabel.Text = "No sticky broadcast was available.";
        }

        await broadcastService.StartListeningAsync();
    }

    private void OnDisappearing(object? sender, EventArgs e)
    {
        broadcastService.BroadcastReceived -= OnBroadcastReceived;
        broadcastService.StopListening();
        initialized = false;
    }

    private void OnBroadcastReceived(object? sender, GlucoseReading reading)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateUi(reading, fromStartup: false));
    }

    private void UpdateUi(GlucoseReading reading, bool fromStartup)
    {
        ValueLabel.Text = reading.ValueMgdl.HasValue
            ? $"Glucose: {reading.ValueMgdl:0} mg/dL"
            : "Glucose: --";

        var slope = reading.Slope.HasValue
            ? $"{reading.Slope.Value:F2} ({reading.SlopeName ?? ""})"
            : reading.SlopeName ?? "--";
        SlopeLabel.Text = $"Slope: {slope}";

        var battery = reading.SensorBattery.HasValue
            ? $", battery {reading.SensorBattery}%"
            : string.Empty;
        SourceLabel.Text = $"Source: {reading.SourceDescription ?? "Unknown"}{battery}";

        TimestampLabel.Text = reading.TimestampMs.HasValue
            ? $"Time: {DateTimeOffset.FromUnixTimeMilliseconds(reading.TimestampMs.Value).ToLocalTime():g}"
            : "Time: --";

        if (reading.Extras.Count > 0)
        {
            RawLabel.Text = "Extras: " + string.Join(", ", reading.Extras.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
        else
        {
            RawLabel.Text = "Extras: none";
        }

        StatusLabel.Text = fromStartup
            ? "Loaded last broadcast when the app started."
            : "Received a live broadcast from xDrip.";
    }
}
