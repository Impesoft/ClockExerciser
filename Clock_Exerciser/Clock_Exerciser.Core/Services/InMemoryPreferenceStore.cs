using Clock_Exerciser.Core.Abstractions;

namespace Clock_Exerciser.Core.Services;

public sealed class InMemoryPreferenceStore : IPreferenceStore
{
    private readonly Dictionary<string, string> _values = [];

    public ValueTask<int> GetIntAsync(string key, int defaultValue, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : defaultValue);
    }

    public ValueTask SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
    {
        _values[key] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return ValueTask.CompletedTask;
    }

    public ValueTask<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        _values.TryGetValue(key, out var value);
        return ValueTask.FromResult(value);
    }

    public ValueTask SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        _values[key] = value;
        return ValueTask.CompletedTask;
    }
}
