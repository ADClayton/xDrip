using System.Text.Json;
using MauiGlucoseReceiver.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Storage;

namespace MauiGlucoseReceiver.Services;

public partial class GlucoseBroadcastService
{
    private const string LatestReadingPreferenceKey = "latest_glucose_reading";
    private const string LatestStatusPreferenceKey = "latest_status_message";

    private readonly ILogger<GlucoseBroadcastService> _logger;

    public GlucoseBroadcastService(ILogger<GlucoseBroadcastService>? logger = null)
    {
        _logger = logger ?? NullLogger<GlucoseBroadcastService>.Instance;
    }

    public event EventHandler<GlucoseReading>? ReadingReceived;
    public event EventHandler<string>? StatusReceived;

    public void Start()
    {
        try
        {
            _logger.LogInformation("Starting glucose broadcast listener.");
            PlatformStartListening();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start glucose broadcast listener.");
        }
    }

    public void Stop()
    {
        try
        {
            _logger.LogInformation("Stopping glucose broadcast listener.");
            PlatformStopListening();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop glucose broadcast listener.");
        }
    }

    partial void PlatformStartListening();
    partial void PlatformStopListening();

    internal void OnReadingReceived(GlucoseReading reading)
    {
        try
        {
            PersistReading(reading);
            ReadingReceived?.Invoke(this, reading);
            _logger.LogInformation(
                "Glucose reading received: {Value} {Units} at {Timestamp} (raw: {Raw}, slope: {Slope})",
                reading.ValueMgDl,
                reading.Units,
                reading.Timestamp,
                reading.Raw,
                reading.SlopeName ?? reading.Slope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling glucose reading broadcast.");
        }
    }

    internal void OnStatusReceived(string status)
    {
        try
        {
            PersistStatus(status);
            StatusReceived?.Invoke(this, status);
            _logger.LogInformation("Collector status received: {Status}", status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling status broadcast.");
        }
    }

    public GlucoseReading? LoadPersistedReading()
    {
        try
        {
            var rawJson = Preferences.Get(LatestReadingPreferenceKey, string.Empty);
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return null;
            }

            var reading = JsonSerializer.Deserialize<GlucoseReading>(rawJson);
            return reading;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load persisted glucose reading.");
            return null;
        }
    }

    public string? LoadPersistedStatus()
    {
        try
        {
            return Preferences.Get(LatestStatusPreferenceKey, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load persisted collector status.");
            return null;
        }
    }

    private void PersistReading(GlucoseReading reading)
    {
        try
        {
            var json = JsonSerializer.Serialize(reading);
            Preferences.Set(LatestReadingPreferenceKey, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to persist glucose reading.");
        }
    }

    private void PersistStatus(string status)
    {
        try
        {
            Preferences.Set(LatestStatusPreferenceKey, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to persist collector status.");
        }
    }
}
