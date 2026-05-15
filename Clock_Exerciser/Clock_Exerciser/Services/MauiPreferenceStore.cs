using Clock_Exerciser.Core.Abstractions;

namespace Clock_Exerciser.Services;

public sealed class MauiPreferenceStore : IPreferenceStore
{
    public ValueTask<int> GetIntAsync(string key, int defaultValue, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(Preferences.Get(key, defaultValue));
    }

    public ValueTask SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
    {
        Preferences.Set(key, value);
        return ValueTask.CompletedTask;
    }

    public ValueTask<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(Preferences.Get(key, null));
    }

    public ValueTask SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        Preferences.Set(key, value);
        return ValueTask.CompletedTask;
    }
}
