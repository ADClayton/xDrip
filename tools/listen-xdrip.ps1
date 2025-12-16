param(
    [string]$DeviceId
)

$adbArgs = @()
if ($DeviceId) {
    $adbArgs += "-s"
    $adbArgs += $DeviceId
}

Write-Host "Clearing existing logcat entries for a focused view..." -ForegroundColor Yellow
& adb @adbArgs "logcat" "-c"

Write-Host "Listening for xDrip broadcasts and receiver logs (press Ctrl+C to stop)..." -ForegroundColor Cyan
& adb @adbArgs "logcat" "-v" "time" "-s" "ActivityManager" "XdripBroadcastReceiver"
