param(
    [string]$Action = "com.eveningoutpost.dexdrip.BROADCAST",
    [string]$ReceiverTag = "XdripBroadcastReceiver"
)

if (-not (Get-Command adb -ErrorAction SilentlyContinue)) {
    Write-Error "adb is required on PATH to monitor device logs."
    exit 1
}

Write-Host "Starting adb server (if needed)..." -ForegroundColor Cyan
& adb start-server | Out-Null

Write-Host "Clearing existing logcat buffer..." -ForegroundColor Cyan
& adb logcat -c | Out-Null

Write-Host "Listening for broadcasts with action '$Action' and receiver logs tagged '$ReceiverTag'. Press Ctrl+C to exit." -ForegroundColor Green

& adb logcat -v time |
    ForEach-Object {
        if ($_ -match [Regex]::Escape($Action) -or $_ -match [Regex]::Escape($ReceiverTag)) {
            Write-Host $_
        }
    }
