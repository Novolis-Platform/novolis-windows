using NAudio.Wave;

namespace Novolis.Windows.Audio;

/// <summary>Captures the default render endpoint through WASAPI loopback.</summary>
public sealed class WindowsLoopbackAudioCapture : IAsyncDisposable
{
    private WasapiLoopbackCapture? _capture;

    /// <summary>Raised for each captured PCM block.</summary>
    public event EventHandler<WaveInEventArgs>? DataAvailable;

    /// <summary>Raised when capture stops or faults.</summary>
    public event EventHandler<StoppedEventArgs>? Stopped;

    /// <summary>Format of the active loopback stream.</summary>
    public WaveFormat? Format => _capture?.WaveFormat;

    /// <summary>Starts capturing the default render endpoint.</summary>
    public void Start()
    {
        if (_capture is not null)
            return;

        var capture = new WasapiLoopbackCapture();
        capture.DataAvailable += OnDataAvailable;
        capture.RecordingStopped += OnStopped;
        _capture = capture;
        capture.StartRecording();
    }

    /// <summary>Stops capture and releases its WASAPI endpoint.</summary>
    public async ValueTask DisposeAsync()
    {
        var capture = Interlocked.Exchange(ref _capture, null);
        if (capture is null)
            return;

        capture.DataAvailable -= OnDataAvailable;
        capture.RecordingStopped -= OnStopped;
        capture.StopRecording();
        capture.Dispose();
        await ValueTask.CompletedTask;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs args) =>
        DataAvailable?.Invoke(this, args);

    private void OnStopped(object? sender, StoppedEventArgs args) =>
        Stopped?.Invoke(this, args);
}
