using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ClockExerciser.Models;

public sealed class LanguageOption : INotifyPropertyChanged
{
    private string _displayName;

    public LanguageOption(string nativeName, string cultureName)
    {
        NativeName = nativeName;
        Culture = new CultureInfo(cultureName);
        _displayName = nativeName;
    }

    public string NativeName { get; }
    public CultureInfo Culture { get; }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public override string ToString() => DisplayName;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (field == value)
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
