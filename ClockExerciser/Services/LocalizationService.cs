using System;
using System.ComponentModel;
using System.Globalization;
using ClockExerciser.Resources.Strings;

namespace ClockExerciser.Services;

public class LocalizationService : INotifyPropertyChanged
{
    readonly CultureInfo _defaultCulture = new("en-US");
    CultureInfo _currentCulture;

    public LocalizationService()
    {
        _currentCulture = _defaultCulture;
        ApplyCulture(_currentCulture);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<CultureInfo>? CultureChanged;

    public CultureInfo CurrentCulture => _currentCulture;

    public void SetCulture(CultureInfo culture)
    {
        if (culture.Name.Equals(_currentCulture.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _currentCulture = culture;
        ApplyCulture(culture);
        CultureChanged?.Invoke(this, culture);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
    }

    public string GetString(string key)
    {
        return AppResources.ResourceManager.GetString(key, _currentCulture) ?? key;
    }

    void ApplyCulture(CultureInfo culture)
    {
        AppResources.Culture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
