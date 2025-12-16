using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;

namespace XdripMauiListener.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true, Name = "com.example.xdripmauilistener.XdripBroadcastReceiver")]
[IntentFilter(new[]
{
    "com.eveningoutpost.dexdrip.BgEstimate",
    "com.eveningoutpost.dexdrip.BgEstimateNoData",
    "com.eveningoutpost.dexdrip.StatusUpdate"
}, Priority = (int)IntentFilterPriority.HighPriority)]
public class XdripBroadcastReceiver : BroadcastReceiver
{
    private const string LogTag = "XdripBroadcastReceiver";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent == null)
        {
            Log.Warn(LogTag, "Received an empty intent from xDrip");
            return;
        }

        var action = intent.Action ?? "<no action>";
        var extrasDescription = DescribeExtras(intent.Extras);

        Log.Info(LogTag, $"Received xDrip broadcast: action={action}; extras={extrasDescription}");
    }

    private static string DescribeExtras(Bundle? extras)
    {
        if (extras == null || extras.IsEmpty)
        {
            return "<none>";
        }

        var builder = new StringBuilder();
        foreach (var key in extras.KeySet() ?? Array.Empty<string>())
        {
            var value = extras.Get(key);
            builder.Append($"{key}={value}; ");
        }

        return builder.ToString().Trim();
    }
}
