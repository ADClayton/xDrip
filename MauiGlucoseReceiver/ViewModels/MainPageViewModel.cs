using System.Collections.ObjectModel;
using System.ComponentModel;
using MauiGlucoseReceiver.Models;
using MauiGlucoseReceiver.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MauiGlucoseReceiver.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly GlucoseBroadcastService _broadcastService;
    private readonly ILogger<MainPageViewModel> _logger;
    private GlucoseReading? _latestReading;
    private string _statusMessage = "Waiting for broadcasts...";

    public MainPageViewModel()
        : this(new GlucoseBroadcastService(), NullLogger<MainPageViewModel>.Instance)
    {
    }

    public MainPageViewModel(GlucoseBroadcastService broadcastService, ILogger<MainPageViewModel> logger)
    {
        _broadcastService = broadcastService;
        _logger = logger;
        Readings = new ObservableCollection<GlucoseReading>();

        _broadcastService.ReadingReceived += OnReadingReceived;
        _broadcastService.StatusReceived += OnStatusReceived;
        _broadcastService.Start();

        LoadPersistedState();
    }

    public ObservableCollection<GlucoseReading> Readings { get; }

    public GlucoseReading? LatestReading
    {
        get => _latestReading;
        private set
        {
            if (_latestReading != value)
            {
                _latestReading = value;
                OnPropertyChanged(nameof(LatestReading));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    private void OnReadingReceived(object? sender, GlucoseReading reading)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LatestReading = reading;
                Readings.Insert(0, reading);

                if (Readings.Count > 50)
                {
                    Readings.RemoveAt(Readings.Count - 1);
                }

                StatusMessage = "Latest reading received.";
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update UI with new glucose reading.");
            StatusMessage = "Unable to display latest reading.";
        }
    }

    private void OnStatusReceived(object? sender, string status)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusMessage = status;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update status message from broadcast.");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void LoadPersistedState()
    {
        try
        {
            var persistedReading = _broadcastService.LoadPersistedReading();
            if (persistedReading != null)
            {
                LatestReading = persistedReading;
                Readings.Insert(0, persistedReading);
            }

            var status = _broadcastService.LoadPersistedStatus();
            if (!string.IsNullOrWhiteSpace(status))
            {
                StatusMessage = status!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load persisted glucose receiver state.");
        }
    }
}
