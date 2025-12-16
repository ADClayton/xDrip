using MauiGlucoseReceiver.Models;

namespace MauiGlucoseReceiver.Services;

public partial class GlucoseBroadcastService
{
    public event EventHandler<GlucoseReading>? ReadingReceived;
    public event EventHandler<string>? StatusReceived;

    public void Start() => PlatformStartListening();

    public void Stop() => PlatformStopListening();

    partial void PlatformStartListening();
    partial void PlatformStopListening();

    internal void OnReadingReceived(GlucoseReading reading) => ReadingReceived?.Invoke(this, reading);

    internal void OnStatusReceived(string status) => StatusReceived?.Invoke(this, status);
}
