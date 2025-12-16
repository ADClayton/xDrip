namespace MauiXdripListener.Services;

public interface IGlucoseBroadcastService
{
    event EventHandler<GlucoseReading>? BroadcastReceived;

    Task<GlucoseReading?> TryGetLastBroadcastAsync(CancellationToken cancellationToken = default);

    Task StartListeningAsync(CancellationToken cancellationToken = default);

    void StopListening();
}

public class GlucoseReading
{
    public double? ValueMgdl { get; init; }
    public double? Slope { get; init; }
    public string? SlopeName { get; init; }
    public int? SensorBattery { get; init; }
    public long? TimestampMs { get; init; }
    public string? SourceDescription { get; init; }
    public string? SourceInfo { get; init; }
    public double? RawNoise { get; init; }
    public string? VersionInfo { get; init; }
    public IDictionary<string, string> Extras { get; init; } = new Dictionary<string, string>();
}
