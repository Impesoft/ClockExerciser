using System.Globalization;
using Clock_Exerciser.Core.Abstractions;
using Microsoft.JSInterop;

namespace Clock_Exerciser.Web.Client.Services;

public sealed class BrowserPreferenceStore : IPreferenceStore, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public BrowserPreferenceStore(IJSRuntime jsRuntime)
    {
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            jsRuntime.InvokeAsync<IJSObjectReference>("import", "./browserStorage.js").AsTask());
    }

    public async ValueTask<int> GetIntAsync(string key, int defaultValue, CancellationToken cancellationToken = default)
    {
        var value = await GetStringAsync(key, cancellationToken);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : defaultValue;
    }

    public async ValueTask SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
    {
        await SetStringAsync(key, value.ToString(CultureInfo.InvariantCulture), cancellationToken);
    }

    public async ValueTask<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string?>("getItem", cancellationToken, key);
    }

    public async ValueTask SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("setItem", cancellationToken, key, value);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_moduleTask.IsValueCreated)
        {
            return;
        }

        var module = await _moduleTask.Value;
        await module.DisposeAsync();
    }
}
