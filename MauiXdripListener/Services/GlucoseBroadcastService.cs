using System.Globalization;

namespace MauiXdripListener.Services;

public class GlucoseBroadcastService : IGlucoseBroadcastService
{
    public event EventHandler<GlucoseReading>? BroadcastReceived;

#if ANDROID
    private const string ActionNewBgEstimate = "com.eveningoutpost.dexdrip.BgEstimate";
    private const string ExtraBgEstimate = "com.eveningoutpost.dexdrip.Extras.BgEstimate";
    private const string ExtraBgSlope = "com.eveningoutpost.dexdrip.Extras.BgSlope";
    private const string ExtraBgSlopeName = "com.eveningoutpost.dexdrip.Extras.BgSlopeName";
    private const string ExtraSensorBattery = "com.eveningoutpost.dexdrip.Extras.SensorBattery";
    private const string ExtraTimestamp = "com.eveningoutpost.dexdrip.Extras.Time";
    private const string ExtraSourceDescription = "com.eveningoutpost.dexdrip.Extras.SourceDesc";
    private const string ExtraSourceInfo = "com.eveningoutpost.dexdrip.Extras.SourceInfo";
    private const string ExtraNoise = "com.eveningoutpost.dexdrip.Extras.Noise";
    private const string ExtraVersion = "com.eveningoutpost.dexdrip.Extras.VersionInfo";
    private const string ReceivePermission = "com.eveningoutpost.dexdrip.permissions.RECEIVE_BG_ESTIMATE";

    private AndroidBroadcastReceiver? _receiver;
#endif

    public Task<GlucoseReading?> TryGetLastBroadcastAsync(CancellationToken cancellationToken = default)
    {
#if ANDROID
        var context = Android.App.Application.Context;
        using var filter = new Android.Content.IntentFilter(ActionNewBgEstimate);
        var stickyIntent = context.RegisterReceiver(receiver: null, filter, ReceivePermission, scheduler: null);
        var reading = ParseIntent(stickyIntent);
        return Task.FromResult(reading);
#else
        return Task.FromResult<GlucoseReading?>(null);
#endif
    }

    public Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
#if ANDROID
        if (_receiver != null)
        {
            return Task.CompletedTask;
        }

        var context = Android.App.Application.Context;
        _receiver = new AndroidBroadcastReceiver(this);
        var filter = new Android.Content.IntentFilter(ActionNewBgEstimate);
        context.RegisterReceiver(_receiver, filter, ReceivePermission, scheduler: null);
#endif
        return Task.CompletedTask;
    }

    public void StopListening()
    {
#if ANDROID
        if (_receiver != null)
        {
            try
            {
                Android.App.Application.Context.UnregisterReceiver(_receiver);
            }
            catch (Java.Lang.IllegalArgumentException)
            {
                // already unregistered
            }
            _receiver.Dispose();
            _receiver = null;
        }
#endif
    }

#if ANDROID
    private void RaiseReading(GlucoseReading? reading)
    {
        if (reading != null)
        {
            BroadcastReceived?.Invoke(this, reading);
        }
    }

    private static GlucoseReading? ParseIntent(Android.Content.Intent? intent)
    {
        if (intent?.Extras == null)
        {
            return null;
        }

        var extras = intent.Extras;

        var reading = new GlucoseReading
        {
            ValueMgdl = TryGetDouble(extras, ExtraBgEstimate),
            Slope = TryGetDouble(extras, ExtraBgSlope),
            SlopeName = extras.GetString(ExtraBgSlopeName),
            SensorBattery = extras.ContainsKey(ExtraSensorBattery) ? (int?)extras.GetInt(ExtraSensorBattery) : null,
            TimestampMs = extras.ContainsKey(ExtraTimestamp) ? (long?)extras.GetLong(ExtraTimestamp) : null,
            SourceDescription = extras.GetString(ExtraSourceDescription),
            SourceInfo = extras.GetString(ExtraSourceInfo),
            RawNoise = TryGetDouble(extras, ExtraNoise),
            VersionInfo = extras.GetString(ExtraVersion),
            Extras = extras.KeySet()?.ToArray()?
                .ToDictionary(key => key, key => ConvertExtraToString(extras, key))
                ?? new Dictionary<string, string>()
        };

        return reading;
    }

    private static double? TryGetDouble(Android.OS.Bundle bundle, string key)
    {
        if (!bundle.ContainsKey(key))
        {
            return null;
        }

        return bundle.GetDouble(key);
    }

    private static string ConvertExtraToString(Android.OS.Bundle bundle, string key)
    {
        var value = bundle.Get(key);
        return value switch
        {
            Java.Lang.ICharSequence seq => seq.ToString(),
            Java.Lang.Number num => num.DoubleValue().ToString(CultureInfo.InvariantCulture),
            null => "",
            _ => value.ToString() ?? string.Empty
        };
    }

    private sealed class AndroidBroadcastReceiver : Android.Content.BroadcastReceiver
    {
        private readonly GlucoseBroadcastService service;

        public AndroidBroadcastReceiver(GlucoseBroadcastService service)
        {
            this.service = service;
        }

        public override void OnReceive(Android.Content.Context? context, Android.Content.Intent? intent)
        {
            var reading = ParseIntent(intent);
            service.RaiseReading(reading);
        }
    }
#endif
}
