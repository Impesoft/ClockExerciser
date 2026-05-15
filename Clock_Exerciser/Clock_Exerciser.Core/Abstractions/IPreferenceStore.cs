namespace Clock_Exerciser.Core.Abstractions;

public interface IPreferenceStore
{
    ValueTask<int> GetIntAsync(string key, int defaultValue, CancellationToken cancellationToken = default);

    ValueTask SetIntAsync(string key, int value, CancellationToken cancellationToken = default);

    ValueTask<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);

    ValueTask SetStringAsync(string key, string value, CancellationToken cancellationToken = default);
}
