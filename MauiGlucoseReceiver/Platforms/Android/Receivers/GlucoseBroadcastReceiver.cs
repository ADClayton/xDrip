#if ANDROID
using Android.Content;
using Android.Util;
using MauiGlucoseReceiver.Services;
using Microsoft.Extensions.Logging;

namespace MauiGlucoseReceiver.Platforms.Android.Receivers;

[BroadcastReceiver(Enabled = true, Exported = true, Permission = GlucoseBroadcastService.ReceiverPermission, DirectBootAware = true)]
[IntentFilter(new[] { GlucoseBroadcastService.ActionNewBgEstimate, GlucoseBroadcastService.ActionStatusUpdate })]
public class GlucoseBroadcastReceiver : BroadcastReceiver
{
    private readonly ILogger<GlucoseBroadcastService> _logger;

    public GlucoseBroadcastReceiver()
    {
        _logger = LoggerFactory.Create(builder => builder.AddDebug())
            .CreateLogger<GlucoseBroadcastService>();
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent == null || intent.Action == null)
        {
            Log.Warn(nameof(GlucoseBroadcastReceiver), "Received empty broadcast intent.");
            return;
        }

        try
        {
            var service = new GlucoseBroadcastService(_logger);

            if (intent.Action.Equals(GlucoseBroadcastService.ActionNewBgEstimate, StringComparison.OrdinalIgnoreCase))
            {
                var reading = GlucoseBroadcastService.ParseReading(intent, _logger);
                service.OnReadingReceived(reading);
            }
            else if (intent.Action.Equals(GlucoseBroadcastService.ActionStatusUpdate, StringComparison.OrdinalIgnoreCase))
            {
                var status = intent.GetStringExtra(GlucoseBroadcastService.ExtraCollectorStatus);
                if (!string.IsNullOrWhiteSpace(status))
                {
                    service.OnStatusReceived(status!);
                }
                else
                {
                    _logger.LogDebug("Manifest receiver received status broadcast without status extra.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process xDrip broadcast from manifest receiver.");
        }
    }
}
#endif
