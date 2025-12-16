using System.Collections.ObjectModel;
using System.ComponentModel;
using MauiGlucoseReceiver.Models;
using MauiGlucoseReceiver.Services;
using Microsoft.Maui.ApplicationModel;

namespace MauiGlucoseReceiver.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly GlucoseBroadcastService _broadcastService;
    private GlucoseReading? _latestReading;
    private string _statusMessage = "Waiting for broadcasts...";

    public MainPageViewModel()
        : this(new GlucoseBroadcastService())
    {
    }

    public MainPageViewModel(GlucoseBroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
        Readings = new ObservableCollection<GlucoseReading>();

        _broadcastService.ReadingReceived += OnReadingReceived;
        _broadcastService.StatusReceived += OnStatusReceived;
        _broadcastService.Start();
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

    private void OnStatusReceived(object? sender, string status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = status;
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
