namespace Clock_Exerciser.Core.Abstractions;

public interface IClockTicker : IAsyncDisposable
{
    event EventHandler<int>? SecondChanged;

    int CurrentSecond { get; }

    void Start();

    void Stop();
}
