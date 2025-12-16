using System;
using Android.Content;
using Android.OS;
using Android.Util;

namespace MauiXdripReceiver.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
[IntentFilter(new[] { "com.eveningoutpost.dexdrip.BROADCAST" })]
public class XdripBroadcastReceiver : BroadcastReceiver
{
    private const string Tag = "XdripBroadcastReceiver";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent is null)
        {
            Log.Warn(Tag, "Received broadcast with null intent");
            return;
        }

        var action = intent.Action ?? "(no action)";
        Log.Info(Tag, $"Received xDrip broadcast. Action: {action}");

        if (intent.Extras is Bundle extras)
        {
            foreach (var key in extras.KeySet() ?? Array.Empty<string>())
            {
                var value = extras.Get(key);
                Log.Debug(Tag, $"Extra: {key}={value}");
            }
        }
        else
        {
            Log.Debug(Tag, "No extras attached to broadcast");
        }
    }
}
