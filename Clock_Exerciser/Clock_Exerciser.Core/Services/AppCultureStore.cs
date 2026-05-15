using System.Globalization;
using Clock_Exerciser.Core.Abstractions;

namespace Clock_Exerciser.Core.Services;

public sealed class AppCultureStore : ICultureStore
{
    private readonly IPreferenceStore _preferenceStore;
    private CultureInfo _currentCulture = new("en-US");

    public AppCultureStore(IPreferenceStore preferenceStore)
    {
        _preferenceStore = preferenceStore;
        ApplyCulture(_currentCulture);
    }

    public event EventHandler<CultureInfo>? CultureChanged;

    public CultureInfo CurrentCulture => _currentCulture;

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        var cultureName = await _preferenceStore.GetStringAsync("Culture", cancellationToken);
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            await SetCultureAsync(new CultureInfo(cultureName), cancellationToken);
        }
    }

    public async ValueTask SetCultureAsync(CultureInfo culture, CancellationToken cancellationToken = default)
    {
        if (culture.Name.Equals(_currentCulture.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _currentCulture = culture;
        ApplyCulture(culture);
        await _preferenceStore.SetStringAsync("Culture", culture.Name, cancellationToken);
        CultureChanged?.Invoke(this, culture);
    }

    private static void ApplyCulture(CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
