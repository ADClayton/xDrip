# Glucose Broadcast Viewer (MAUI)

A lightweight .NET MAUI companion app that listens for the glucose broadcast intents emitted by xDrip+ and displays the latest readings.

## Receiving xDrip+ broadcasts

xDrip+ can send glucose updates as Android broadcasts when the **Broadcast locally collected data** option is enabled. The viewer listens for `com.eveningoutpost.dexdrip.BgEstimate` and `com.eveningoutpost.dexdrip.StatusUpdate` actions and understands the extras defined in `app/src/main/java/com/eveningoutpost/dexdrip/utilitymodels/Intents.java`.

The Android manifest declares the required permission `com.eveningoutpost.dexdrip.permissions.RECEIVE_BG_ESTIMATE`, matching the permission enforced by `SendXdripBroadcast`.

To continue receiving broadcasts even when the UI is closed, Android registers a manifest-declared `BroadcastReceiver` for these intents. The receiver persists the last reading and status locally so they can be displayed immediately when the app is launched again.

## Building

The project targets .NET 8 MAUI (`net8.0-android`, `net8.0-ios`, `net8.0-maccatalyst`). Open `MauiGlucoseReceiver.sln` with Visual Studio 2022 or `dotnet build` from the command line with the MAUI workload installed. Android is the primary target to receive the xDrip+ broadcasts.

## UI overview

- Displays the latest glucose value, units, timestamp, trend name, and raw value when provided.
- Shows incoming collector status messages when available.
- Maintains a short history of recent broadcasts for quick review.
- Persists the last received reading and status for display on subsequent launches.
