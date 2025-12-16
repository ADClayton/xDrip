[CmdletBinding()]
param(
    [string]$PackageId = "com.eveningoutpost.dexdrip.mauireceiver",
    [string]$OutputRoot = "./logs",
    [switch]$IncludeFullLogcat,
    [int]$TailLines = 4000
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $Message"
}

function Get-AdbPath {
    try {
        $adb = Get-Command adb -ErrorAction Stop
        return $adb.Source
    }
    catch {
        throw "adb not found on PATH. Install the Android platform tools and ensure adb is available before running this script."
    }
}

$adbPath = Get-AdbPath()
Write-Step "Using adb at $adbPath"

adb start-server | Out-Null

$attachedDevices = adb devices | Select-String "\tdevice$"
if (-not $attachedDevices) {
    throw "No connected Android devices found. Connect a device with USB debugging or ensure an emulator is running."
}

Write-Step "Connected device(s): $($attachedDevices -join ', ')"

if (-not (Test-Path -Path $OutputRoot)) {
    New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$sessionDir = Join-Path -Path (Resolve-Path $OutputRoot) -ChildPath "maui-receiver-logs-$timestamp"
New-Item -ItemType Directory -Path $sessionDir -Force | Out-Null

adb devices > (Join-Path $sessionDir "devices.txt")
adb shell getprop > (Join-Path $sessionDir "device-properties.txt")
adb shell dumpsys package $PackageId > (Join-Path $sessionDir "package-dumpsys.txt")

$pid = (adb shell pidof -s $PackageId 2>$null).Trim()
if ($pid) {
    Write-Step "Application PID: $pid"
} else {
    Write-Warning "Application PID not found. Manifest receivers may still log when broadcasts arrive."
}

$filteredLogFile = Join-Path $sessionDir "logcat-filtered.txt"
$logcatArgs = @("logcat", "-v", "threadtime", "-d")
if ($pid) {
    $logcatArgs += "--pid=$pid"
}

if ($pid) {
    & adb @logcatArgs | Set-Content -Path $filteredLogFile
} else {
    & adb @logcatArgs |
        Select-String -Pattern $PackageId, "GlucoseBroadcastService", "GlucoseBroadcastReceiver", "dexdrip" |
        ForEach-Object { $_.Line } |
        Set-Content -Path $filteredLogFile
}

if ($IncludeFullLogcat.IsPresent) {
    $fullLogFile = Join-Path $sessionDir "logcat-full.txt"
    & adb @("logcat", "-v", "threadtime", "-d") |
        Select-Object -Last $TailLines |
        Set-Content -Path $fullLogFile
    Write-Step "Captured full logcat tail (last $TailLines lines)."
}

Write-Step "Logs collected in $sessionDir"
