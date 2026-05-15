using Clock_Exerciser.Core.Abstractions;

namespace Clock_Exerciser.Core.Services;

public sealed class SystemClockTicker : IClockTicker
{
    private Timer? _timer;
    private bool _disposed;

    public event EventHandler<int>? SecondChanged;

    public int CurrentSecond { get; private set; } = DateTime.Now.Second;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _timer ??= new Timer(OnTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
        await ValueTask.CompletedTask;
    }

    private void OnTick(object? state)
    {
        CurrentSecond = (CurrentSecond + 1) % 60;
        SecondChanged?.Invoke(this, CurrentSecond);
    }
}
