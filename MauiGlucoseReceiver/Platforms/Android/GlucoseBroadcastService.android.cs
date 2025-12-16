#if ANDROID
using Android.Content;
using Android.OS;
using Android.Util;
using MauiGlucoseReceiver.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MauiGlucoseReceiver.Services;

public partial class GlucoseBroadcastService
{
    internal const string ActionNewBgEstimate = "com.eveningoutpost.dexdrip.BgEstimate";
    internal const string ActionStatusUpdate = "com.eveningoutpost.dexdrip.StatusUpdate";
    internal const string ReceiverPermission = "com.eveningoutpost.dexdrip.permissions.RECEIVE_BG_ESTIMATE";

    internal const string ExtraBgEstimate = "com.eveningoutpost.dexdrip.Extras.BgEstimate";
    internal const string ExtraBgSlope = "com.eveningoutpost.dexdrip.Extras.BgSlope";
    internal const string ExtraBgSlopeName = "com.eveningoutpost.dexdrip.Extras.BgSlopeName";
    internal const string ExtraTimestamp = "com.eveningoutpost.dexdrip.Extras.Time";
    internal const string ExtraRaw = "com.eveningoutpost.dexdrip.Extras.Raw";
    internal const string ExtraDisplayUnits = "com.eveningoutpost.dexdrip.Extras.Display.Units";
    internal const string ExtraSourceDescription = "com.eveningoutpost.dexdrip.Extras.SourceDesc";
    internal const string ExtraSourceInfo = "com.eveningoutpost.dexdrip.Extras.SourceInfo";
    internal const string ExtraCollectorStatus = "com.eveningoutpost.dexdrip.Extras.Collector.NanoStatus";

    private const string DefaultUnits = "mg/dL";

    private ReadingBroadcastReceiver? _receiver;

    partial void PlatformStartListening()
    {
        if (_receiver != null)
        {
            return;
        }

        try
        {
            _receiver = new ReadingBroadcastReceiver(this, _logger);
            var filter = new IntentFilter();
            filter.AddAction(ActionNewBgEstimate);
            filter.AddAction(ActionStatusUpdate);

            Application.Context.RegisterReceiver(_receiver, filter, ReceiverPermission, null);
            _logger.LogInformation("Registered runtime broadcast receiver for xDrip intents.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register runtime broadcast receiver.");
        }
    }

    partial void PlatformStopListening()
    {
        if (_receiver == null)
        {
            return;
        }

        try
        {
            Application.Context.UnregisterReceiver(_receiver);
            _receiver.Dispose();
            _receiver = null;
            _logger.LogInformation("Unregistered runtime broadcast receiver.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister runtime broadcast receiver.");
        }
    }

    private class ReadingBroadcastReceiver : BroadcastReceiver
    {
        private readonly GlucoseBroadcastService _service;
        private readonly ILogger<GlucoseBroadcastService> _logger;

        public ReadingBroadcastReceiver(GlucoseBroadcastService service, ILogger<GlucoseBroadcastService> logger)
        {
            _service = service;
            _logger = logger;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent == null || intent.Action == null)
            {
                _logger.LogWarning("Received null intent or action in runtime broadcast receiver.");
                return;
            }

            try
            {
                if (intent.Action.Equals(ActionNewBgEstimate, StringComparison.OrdinalIgnoreCase))
                {
                    _service.OnReadingReceived(ParseReading(intent, _logger));
                }
                else if (intent.Action.Equals(ActionStatusUpdate, StringComparison.OrdinalIgnoreCase))
                {
                    var status = intent.GetStringExtra(ExtraCollectorStatus);
                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        _service.OnStatusReceived(status!);
                    }
                    else
                    {
                        _logger.LogDebug("Status broadcast received without collector status extra.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process xDrip broadcast intent at runtime.");
            }
        }
    }

    internal static GlucoseReading ParseReading(Intent intent, ILogger? logger = null)
    {
        var activeLogger = logger ?? NullLogger.Instance;

        var units = intent.GetStringExtra(ExtraDisplayUnits);
        if (string.IsNullOrWhiteSpace(units))
        {
            units = DefaultUnits;
        }

        var timestampMs = intent.GetLongExtra(ExtraTimestamp, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        var reading = new GlucoseReading
        {
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).LocalDateTime,
            ValueMgDl = intent.Extras?.ContainsKey(ExtraBgEstimate) == true
                ? intent.GetDoubleExtra(ExtraBgEstimate, double.NaN)
                : null,
            Raw = intent.Extras?.ContainsKey(ExtraRaw) == true ? intent.GetDoubleExtra(ExtraRaw, double.NaN) : null,
            Slope = intent.Extras?.ContainsKey(ExtraBgSlope) == true ? intent.GetDoubleExtra(ExtraBgSlope, double.NaN) : null,
            SlopeName = intent.GetStringExtra(ExtraBgSlopeName),
            Units = units,
            SourceDescription = intent.GetStringExtra(ExtraSourceDescription),
            SourceInfo = intent.GetStringExtra(ExtraSourceInfo),
            CollectorStatus = intent.GetStringExtra(ExtraCollectorStatus)
        };

        if (reading.ValueMgDl.HasValue && double.IsNaN(reading.ValueMgDl.Value))
        {
            activeLogger.LogDebug("Received NaN value for BgEstimate; treating as null.");
            reading.ValueMgDl = null;
        }

        if (reading.Raw.HasValue && double.IsNaN(reading.Raw.Value))
        {
            activeLogger.LogDebug("Received NaN value for raw reading; treating as null.");
            reading.Raw = null;
        }

        if (reading.Slope.HasValue && double.IsNaN(reading.Slope.Value))
        {
            activeLogger.LogDebug("Received NaN value for slope; treating as null.");
            reading.Slope = null;
        }

        return reading;
    }
}
#endif
