using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using ClockExerciser.Models;
using ClockExerciser.Services;
using Microsoft.Maui.Controls;

namespace ClockExerciser.ViewModels;

public sealed class GameViewModel : INotifyPropertyChanged
{
    private readonly LocalizationService _localizationService;
    private readonly Random _random = new();
    private GameMode _activeMode = GameMode.ClockToTime;
    private TimeSpan _targetTime;
    private string _answerText = string.Empty;
    private string _promptText = string.Empty;
    private string _promptDigital = string.Empty;
    private string _resultMessage = string.Empty;
    private bool _resultVisible;
    private bool _resultSuccess;
    private double _userHourValue = 0;
    private double _userMinuteValue = 0;
    private double _userSecondValue = 0;
    private GameModeOption? _selectedMode;
    private LanguageOption? _selectedLanguage;

    public GameViewModel(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.CultureChanged += (_, _) => OnCultureChanged();

        Languages = new ObservableCollection<LanguageOption>
        {
            new("LanguageDutch", "nl-NL"),
            new("LanguageEnglish", "en-US")
        };

        ModeOptions = new ObservableCollection<GameModeOption>
        {
            new(GameMode.ClockToTime, string.Empty),
            new(GameMode.TimeToClock, string.Empty),
            new(GameMode.Random, string.Empty)
        };

        CheckAnswerCommand = new Command(ExecuteCheckAnswer);
        NextChallengeCommand = new Command(() => GenerateNewChallenge());

        UpdateCultureDependentData();

        SelectedLanguage = Languages.First();
        SelectedMode = ModeOptions.First();
        GenerateNewChallenge();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LanguageOption> Languages { get; }

    public ObservableCollection<GameModeOption> ModeOptions { get; }

    public LanguageOption? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value) && value is not null)
            {
                _localizationService.SetCulture(value.Culture);
            }
        }
    }

    public GameModeOption? SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (SetProperty(ref _selectedMode, value))
            {
                GenerateNewChallenge();
            }
        }
    }

    public string AnswerText
    {
        get => _answerText;
        set
        {
            if (SetProperty(ref _answerText, value))
            {
                OnPropertyChanged(nameof(CanSubmit));
            }
        }
    }

    public string PromptText
    {
        get => _promptText;
        private set => SetProperty(ref _promptText, value);
    }

    public string PromptDigital
    {
        get => _promptDigital;
        private set => SetProperty(ref _promptDigital, value);
    }

    public string ResultMessage
    {
        get => _resultMessage;
        private set => SetProperty(ref _resultMessage, value);
    }

    public bool ResultVisible
    {
        get => _resultVisible;
        private set => SetProperty(ref _resultVisible, value);
    }

    public bool ResultSuccess
    {
        get => _resultSuccess;
        private set => SetProperty(ref _resultSuccess, value);
    }

    public bool CanSubmit => _activeMode == GameMode.ClockToTime ? !string.IsNullOrWhiteSpace(AnswerText) : true;

    public bool IsClockToTime => _activeMode == GameMode.ClockToTime;

    public bool IsTimeToClock => _activeMode == GameMode.TimeToClock;

    public TimeSpan TargetTime
    {
        get => _targetTime;
        private set
        {
            if (SetProperty(ref _targetTime, value))
            {
                UpdatePromptTexts();
                OnPropertyChanged(nameof(HourPointerValue));
                OnPropertyChanged(nameof(MinutePointerValue));
                OnPropertyChanged(nameof(SecondPointerValue));
            }
        }
    }

    public double UserHourValue
    {
        get => _userHourValue;
        set
        {
            if (Math.Abs(_userHourValue - value) < 0.001)
            {
                return;
            }

            _userHourValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HourPointerValue));
        }
    }

    public double UserMinuteValue
    {
        get => _userMinuteValue;
        set
        {
            if (Math.Abs(_userMinuteValue - value) < 0.001)
            {
                return;
            }

            _userMinuteValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MinutePointerValue));
        }
    }

    public double UserSecondValue
    {
        get => _userSecondValue;
        set
        {
            if (Math.Abs(_userSecondValue - value) < 0.001)
            {
                return;
            }

            _userSecondValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SecondPointerValue));
        }
    }

    public double HourPointerValue => IsClockToTime ? GetTargetHourPointer() : UserHourValue;

    public double MinutePointerValue => IsClockToTime ? GetTargetMinutePointer() : UserMinuteValue;

    public double SecondPointerValue => IsClockToTime ? TargetTime.Seconds : UserSecondValue;

    public string AppTitle => _localizationService.GetString("AppTitle");
    public string LanguageLabel => _localizationService.GetString("LanguageLabel");
    public string ModeLabel => _localizationService.GetString("ModeLabel");
    public string EntryPlaceholder => _localizationService.GetString("EntryPlaceholder");
    public string SubmitText => _localizationService.GetString("SubmitAnswer");
    public string NextText => _localizationService.GetString("NextChallenge");
    public string PromptLabel => _localizationService.GetString("PromptLabel");
    public string HourLabel => _localizationService.GetString("HourLabel");
    public string MinuteLabel => _localizationService.GetString("MinuteLabel");
    public string SecondLabel => _localizationService.GetString("SecondLabel");
    public string InstructionText => IsClockToTime ? _localizationService.GetString("ClockToTimeInstruction") : _localizationService.GetString("TimeToClockInstruction");

    public Command CheckAnswerCommand { get; }
    public Command NextChallengeCommand { get; }

    private void ExecuteCheckAnswer()
    {
        if (_activeMode == GameMode.ClockToTime)
        {
            EvaluateTextAnswer();
        }
        else
        {
            EvaluateClockAnswer();
        }
    }

    public void GenerateNewChallenge()
    {
        var requestedMode = SelectedMode?.Mode ?? GameMode.ClockToTime;
        _activeMode = requestedMode == GameMode.Random ? (_random.Next(0, 2) == 0 ? GameMode.ClockToTime : GameMode.TimeToClock) : requestedMode;
        TargetTime = CreateRandomTime();
        AnswerText = string.Empty;
        ResultVisible = false;
        ResultMessage = string.Empty;
        ResultSuccess = false;
        if (IsTimeToClock)
        {
            ResetUserHands();
        }
        OnPropertyChanged(nameof(IsClockToTime));
        OnPropertyChanged(nameof(IsTimeToClock));
        OnPropertyChanged(nameof(InstructionText));
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(HourPointerValue));
        OnPropertyChanged(nameof(MinutePointerValue));
        OnPropertyChanged(nameof(SecondPointerValue));
    }

    private void EvaluateTextAnswer()
    {
        if (!TryParseUserTime(out var userTime))
        {
            ResultVisible = true;
            ResultSuccess = false;
            ResultMessage = _localizationService.GetString("ResultIncorrect");
            return;
        }

        var success = MatchesTime(userTime, TargetTime);
        SetResult(success);
    }

    private void EvaluateClockAnswer()
    {
        var hourDiff = CircularDifference(UserHourValue, GetTargetHourPointer(), 12);
        var minuteDiff = CircularDifference(UserMinuteValue, GetTargetMinutePointer(), 60);
        var secondDiff = CircularDifference(UserSecondValue, TargetTime.Seconds, 60);

        var success = hourDiff <= 0.1 && minuteDiff <= 1 && secondDiff <= 5;
        SetResult(success);
    }

    private void SetResult(bool success)
    {
        ResultSuccess = success;
        ResultMessage = success ? _localizationService.GetString("ResultCorrect") : _localizationService.GetString("ResultIncorrect");
        ResultVisible = true;
    }

    private void ResetUserHands()
    {
        UserHourValue = 0;
        UserMinuteValue = 0;
        UserSecondValue = 0;
    }

    private void UpdatePromptTexts()
    {
        PromptDigital = TargetTime.ToString("hh\\:mm");
        PromptText = FormatFriendlyTime(TargetTime, _localizationService.CurrentCulture);
    }

    private void EvaluateCultureLists()
    {
        foreach (var language in Languages)
        {
            language.UpdateDisplay(_localizationService.GetString);
        }

        foreach (var option in ModeOptions)
        {
            option.UpdateLabel(_localizationService.GetString);
        }
    }

    private void UpdateCultureDependentData()
    {
        EvaluateCultureLists();
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(ModeLabel));
        OnPropertyChanged(nameof(EntryPlaceholder));
        OnPropertyChanged(nameof(SubmitText));
        OnPropertyChanged(nameof(NextText));
        OnPropertyChanged(nameof(PromptLabel));
        OnPropertyChanged(nameof(HourLabel));
        OnPropertyChanged(nameof(MinuteLabel));
        OnPropertyChanged(nameof(SecondLabel));
        OnPropertyChanged(nameof(InstructionText));
    }

    private void OnCultureChanged()
    {
        UpdateCultureDependentData();
        UpdatePromptTexts();
    }

    private TimeSpan CreateRandomTime()
    {
        var hour = _random.Next(0, 24);
        var minute = _random.Next(0, 12) * 5;
        var second = _random.Next(0, 12) * 5;
        return new TimeSpan(hour, minute, second);
    }

    private bool TryParseUserTime(out TimeSpan time)
    {
        var input = AnswerText?.Trim();
        if (string.IsNullOrEmpty(input))
        {
            time = default;
            return false;
        }

        var formats = new[] { "h\\:mm", "hh\\:mm", "H\\:mm", "HH\\:mm" };
        foreach (var format in formats)
        {
            if (TimeSpan.TryParseExact(input, format, CultureInfo.InvariantCulture, out time))
            {
                return true;
            }
        }

        time = default;
        return false;
    }

    private static bool MatchesTime(TimeSpan candidate, TimeSpan target)
    {
        var diff = Math.Abs((candidate - target).TotalMinutes);
        var diffPlus = Math.Abs((candidate - target.Add(TimeSpan.FromHours(12))).TotalMinutes);
        var diffMinus = Math.Abs((candidate - target.Add(TimeSpan.FromHours(-12))).TotalMinutes);
        return diff <= 1 || diffPlus <= 1 || diffMinus <= 1;
    }

    private static double CircularDifference(double a, double b, double period)
    {
        var diff = Math.Abs(a - b);
        return Math.Min(diff, period - diff);
    }

    private double GetTargetHourPointer()
    {
        var hours = TargetTime.Hours % 12;
        return hours + TargetTime.Minutes / 60d + TargetTime.Seconds / 3600d;
    }

    private double GetTargetMinutePointer()
    {
        return TargetTime.Minutes + TargetTime.Seconds / 60d;
    }

    private static string FormatFriendlyTime(TimeSpan time, CultureInfo culture)
    {
        var hour = time.Hours % 12;
        if (hour == 0)
        {
            hour = 12;
        }

        var nextHour = (hour % 12) + 1;
        var minute = time.Minutes;

        if (culture.TwoLetterISOLanguageName.Equals("nl", StringComparison.OrdinalIgnoreCase))
        {
            return CreateDutchPhrase(hour, nextHour, minute);
        }

        return CreateEnglishPhrase(hour, nextHour, minute);
    }

    private static string CreateDutchPhrase(int hour, int nextHour, int minute)
    {
        string HourWord(int value) => value switch
        {
            1 => "een",
            2 => "twee",
            3 => "drie",
            4 => "vier",
            5 => "vijf",
            6 => "zes",
            7 => "zeven",
            8 => "acht",
            9 => "negen",
            10 => "tien",
            11 => "elf",
            12 => "twaalf",
            _ => value.ToString()
        };

        return minute switch
        {
            0 => $"{HourWord(hour)} uur",
            15 => $"kwart over {HourWord(hour)}",
            30 => $"half {HourWord(nextHour)}",
            45 => $"kwart voor {HourWord(nextHour)}",
            _ when minute < 30 => $"{minute} over {HourWord(hour)}",
            _ => $"{60 - minute} voor {HourWord(nextHour)}"
        };
    }

    private static string CreateEnglishPhrase(int hour, int nextHour, int minute)
    {
        string HourWord(int value) => value switch
        {
            1 => "one",
            2 => "two",
            3 => "three",
            4 => "four",
            5 => "five",
            6 => "six",
            7 => "seven",
            8 => "eight",
            9 => "nine",
            10 => "ten",
            11 => "eleven",
            12 => "twelve",
            _ => value.ToString()
        };

        return minute switch
        {
            0 => $"{HourWord(hour)} o'clock",
            15 => $"quarter past {HourWord(hour)}",
            30 => $"half past {HourWord(hour)}",
            45 => $"quarter to {HourWord(nextHour)}",
            _ when minute < 30 => $"{minute} past {HourWord(hour)}",
            _ => $"{60 - minute} to {HourWord(nextHour)}"
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
