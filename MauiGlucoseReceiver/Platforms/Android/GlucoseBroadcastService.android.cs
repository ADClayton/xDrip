#if ANDROID
using Android.Content;
using Android.OS;
using MauiGlucoseReceiver.Models;

namespace MauiGlucoseReceiver.Services;

public partial class GlucoseBroadcastService
{
    private const string ActionNewBgEstimate = "com.eveningoutpost.dexdrip.BgEstimate";
    private const string ActionStatusUpdate = "com.eveningoutpost.dexdrip.StatusUpdate";
    private const string ReceiverPermission = "com.eveningoutpost.dexdrip.permissions.RECEIVE_BG_ESTIMATE";

    private const string ExtraBgEstimate = "com.eveningoutpost.dexdrip.Extras.BgEstimate";
    private const string ExtraBgSlope = "com.eveningoutpost.dexdrip.Extras.BgSlope";
    private const string ExtraBgSlopeName = "com.eveningoutpost.dexdrip.Extras.BgSlopeName";
    private const string ExtraTimestamp = "com.eveningoutpost.dexdrip.Extras.Time";
    private const string ExtraRaw = "com.eveningoutpost.dexdrip.Extras.Raw";
    private const string ExtraDisplayUnits = "com.eveningoutpost.dexdrip.Extras.Display.Units";
    private const string ExtraSourceDescription = "com.eveningoutpost.dexdrip.Extras.SourceDesc";
    private const string ExtraSourceInfo = "com.eveningoutpost.dexdrip.Extras.SourceInfo";
    private const string ExtraCollectorStatus = "com.eveningoutpost.dexdrip.Extras.Collector.NanoStatus";

    private const string DefaultUnits = "mg/dL";

    private ReadingBroadcastReceiver? _receiver;

    partial void PlatformStartListening()
    {
        if (_receiver != null)
        {
            return;
        }

        _receiver = new ReadingBroadcastReceiver(this);
        var filter = new IntentFilter();
        filter.AddAction(ActionNewBgEstimate);
        filter.AddAction(ActionStatusUpdate);

        Application.Context.RegisterReceiver(_receiver, filter, ReceiverPermission, null);
    }

    partial void PlatformStopListening()
    {
        if (_receiver == null)
        {
            return;
        }

        Application.Context.UnregisterReceiver(_receiver);
        _receiver.Dispose();
        _receiver = null;
    }

    private class ReadingBroadcastReceiver : BroadcastReceiver
    {
        private readonly GlucoseBroadcastService _service;

        public ReadingBroadcastReceiver(GlucoseBroadcastService service)
        {
            _service = service;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent == null || intent.Action == null)
            {
                return;
            }

            if (intent.Action.Equals(ActionNewBgEstimate, StringComparison.OrdinalIgnoreCase))
            {
                _service.OnReadingReceived(ParseReading(intent));
            }
            else if (intent.Action.Equals(ActionStatusUpdate, StringComparison.OrdinalIgnoreCase))
            {
                var status = intent.GetStringExtra(ExtraCollectorStatus);
                if (!string.IsNullOrWhiteSpace(status))
                {
                    _service.OnStatusReceived(status!);
                }
            }
        }

        private static GlucoseReading ParseReading(Intent intent)
        {
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
                reading.ValueMgDl = null;
            }

            if (reading.Raw.HasValue && double.IsNaN(reading.Raw.Value))
            {
                reading.Raw = null;
            }

            if (reading.Slope.HasValue && double.IsNaN(reading.Slope.Value))
            {
                reading.Slope = null;
            }

            return reading;
        }
    }
}
#endif
