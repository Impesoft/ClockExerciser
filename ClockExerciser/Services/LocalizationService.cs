using System;
using System.ComponentModel;
using System.Globalization;
using ClockExerciser.Resources.Strings;
using Microsoft.Maui.Storage;

namespace ClockExerciser.Services;

public class LocalizationService : INotifyPropertyChanged
{
    const string PreferenceKey = "SelectedCulture";
    readonly CultureInfo _defaultCulture = new("en-US");
    CultureInfo _currentCulture;

    public LocalizationService()
    {
        // Load saved culture or use default
        var savedCulture = Preferences.Get(PreferenceKey, _defaultCulture.Name);
        _currentCulture = new CultureInfo(savedCulture);
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
        
        // Persist the selected culture
        Preferences.Set(PreferenceKey, culture.Name);
        
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
