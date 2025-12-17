using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClockExerciser.Models;

public sealed class GameModeOption : INotifyPropertyChanged
{
    private string _label;

    public GameModeOption(GameMode mode, string label)
    {
        Mode = mode;
        _label = label;
    }

    public GameMode Mode { get; }

    public string Label
    {
        get => _label;
        private set => SetProperty(ref _label, value);
    }

    public void UpdateLabel(Func<string, string> translator)
    {
        var key = Mode switch
        {
            GameMode.ClockToTime => "ModeClockToTime",
            GameMode.TimeToClock => "ModeTimeToClock",
            _ => "ModeRandom"
        };

        Label = translator(key);
    }

    public override string ToString() => Label;

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
