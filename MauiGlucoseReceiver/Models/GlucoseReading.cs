namespace MauiGlucoseReceiver.Models;

public class GlucoseReading
{
    public DateTime Timestamp { get; set; }
    public double? ValueMgDl { get; set; }
    public double? Raw { get; set; }
    public double? Slope { get; set; }
    public string? SlopeName { get; set; }
    public string? Units { get; set; }
    public string? SourceDescription { get; set; }
    public string? SourceInfo { get; set; }
    public string? CollectorStatus { get; set; }
}
