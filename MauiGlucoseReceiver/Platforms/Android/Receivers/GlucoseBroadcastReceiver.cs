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
                _logger.LogInformation(
                    "Manifest receiver got glucose broadcast with extras: BgEstimate={Bg} Raw={Raw} Slope={Slope} SlopeName={SlopeName} Units={Units} SourceDesc={SourceDesc} SourceInfo={SourceInfo} CollectorStatus={CollectorStatus}",
                    intent.Extras?.ContainsKey(GlucoseBroadcastService.ExtraBgEstimate) == true ? intent.GetDoubleExtra(GlucoseBroadcastService.ExtraBgEstimate, double.NaN) : null,
                    intent.Extras?.ContainsKey(GlucoseBroadcastService.ExtraRaw) == true ? intent.GetDoubleExtra(GlucoseBroadcastService.ExtraRaw, double.NaN) : null,
                    intent.Extras?.ContainsKey(GlucoseBroadcastService.ExtraBgSlope) == true ? intent.GetDoubleExtra(GlucoseBroadcastService.ExtraBgSlope, double.NaN) : null,
                    intent.GetStringExtra(GlucoseBroadcastService.ExtraBgSlopeName),
                    intent.GetStringExtra(GlucoseBroadcastService.ExtraDisplayUnits) ?? GlucoseBroadcastService.DefaultUnits,
                    intent.GetStringExtra(GlucoseBroadcastService.ExtraSourceDescription),
                    intent.GetStringExtra(GlucoseBroadcastService.ExtraSourceInfo),
                    intent.GetStringExtra(GlucoseBroadcastService.ExtraCollectorStatus));
                var reading = GlucoseBroadcastService.ParseReading(intent, _logger);
                service.OnReadingReceived(reading);
            }
            else if (intent.Action.Equals(GlucoseBroadcastService.ActionStatusUpdate, StringComparison.OrdinalIgnoreCase))
            {
                var status = intent.GetStringExtra(GlucoseBroadcastService.ExtraCollectorStatus);
                if (!string.IsNullOrWhiteSpace(status))
                {
                    _logger.LogInformation("Manifest receiver got status broadcast: {Status}", status);
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
