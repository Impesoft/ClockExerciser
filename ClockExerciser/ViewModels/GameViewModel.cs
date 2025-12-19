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
using Microsoft.Maui.Storage;

namespace ClockExerciser.ViewModels;

public sealed class GameViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly LocalizationService _localizationService;
    private readonly IAudioService _audioService;
    private readonly DutchTimeParser _dutchTimeParser;
    private readonly EnglishTimeParser _englishTimeParser;
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
    private LanguageOption? _selectedLanguage;
    private IDispatcherTimer? _secondTimer;
    private int _currentSecond = 0;
    private int _correctAnswers = 0;
    private bool _answerChecked = false;

    public GameViewModel(LocalizationService localizationService, IAudioService audioService, 
        DutchTimeParser dutchTimeParser, EnglishTimeParser englishTimeParser)
    {
        _localizationService = localizationService;
        _audioService = audioService;
        _dutchTimeParser = dutchTimeParser;
        _englishTimeParser = englishTimeParser;
        _localizationService.CultureChanged += (_, _) => OnCultureChanged();

        Languages = new ObservableCollection<LanguageOption>
        {
            new("English", "en-US"),
            new("Nederlands", "nl-NL")
        };

        CheckAnswerCommand = new Command(ExecutePrimaryAction);

        UpdateCultureDependentData();

        // Set to English by default (matches LocalizationService default)
        SelectedLanguage = Languages.First();
        
        // Load saved score
        _correctAnswers = Preferences.Get("CorrectAnswers", 0);
        
        // Start the second hand timer
        StartSecondTimer();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LanguageOption> Languages { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var modeObj) && modeObj is GameMode mode)
        {
            _activeMode = mode;
            GenerateNewChallenge();
        }
    }

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
    
    public int CorrectAnswers
    {
        get => _correctAnswers;
        private set
        {
            if (SetProperty(ref _correctAnswers, value))
            {
                // Persist score
                Preferences.Set("CorrectAnswers", value);
                OnPropertyChanged(nameof(ScoreText));
            }
        }
    }
    
    public string ScoreText => $"? {CorrectAnswers}";
    
    public string PrimaryButtonText => _answerChecked && _resultSuccess 
        ? _localizationService.GetString("NextChallenge") 
        : _localizationService.GetString("SubmitAnswer");
    
    public bool ShowNextButton => false; // Always hidden - using single button now

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

    public double MinutePointerValue => ConvertToDialValue(IsClockToTime ? GetTargetMinuteValue() : UserMinuteValue);

    public double SecondPointerValue => ConvertToDialValue(_currentSecond);  // Always show ticking second hand

    private static double ConvertToDialValue(double rawValue) => rawValue / 5d;

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

    private void ExecutePrimaryAction()
    {
        if (_answerChecked && _resultSuccess)
        {
            // User clicked "Next Challenge" after correct answer
            GenerateNewChallenge();
        }
        else
        {
            // User clicked "Check Answer"
            ExecuteCheckAnswer();
        }
    }
    
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
        
        _answerChecked = true;
        OnPropertyChanged(nameof(PrimaryButtonText));
    }

    public void GenerateNewChallenge()
    {
        _activeMode = _activeMode == GameMode.Random ? (_random.Next(0, 2) == 0 ? GameMode.ClockToTime : GameMode.TimeToClock) : _activeMode;
        TargetTime = CreateRandomTime();
        AnswerText = string.Empty;
        ResultVisible = false;
        ResultMessage = string.Empty;
        ResultSuccess = false;
        _answerChecked = false; // Reset for new challenge
        
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
        OnPropertyChanged(nameof(PrimaryButtonText));
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
        
        // Add debug info for incorrect answers
        if (!success)
        {
            var debugInfo = $" (You: {userTime:hh\\:mm}, Target: {TargetTime:hh\\:mm})";
            SetResult(success, debugInfo);
        }
        else
        {
            SetResult(success);
        }
    }

    private void EvaluateClockAnswer()
    {
        // Get integer hour from user slider (0-12)
        var userHour = (int)UserHourValue;
        // Get integer hour from target (0-11, then wrap 0 to 12)
        var targetHour = TargetTime.Hours % 12;
        if (targetHour == 0) targetHour = 12;
        
        // Get integer minute from user slider (0-59)
        var userMinute = (int)UserMinuteValue;
        var targetMinute = TargetTime.Minutes;
        
        // Hours must match exactly (after normalization)
        var hoursMatch = userHour == targetHour || 
                        (userHour == 0 && targetHour == 12) ||
                        (userHour == 12 && targetHour == 0);
        
        // Minutes within 1 minute tolerance
        var minuteDiff = Math.Abs(userMinute - targetMinute);
        var minutesMatch = minuteDiff <= 1;
        
        var success = hoursMatch && minutesMatch;
        
        // Add debug info for incorrect answers
        if (!success)
        {
            var userTime = new TimeSpan(userHour, userMinute, 0);
            var debugInfo = $" (You: {userTime:hh\\:mm}, Target: {TargetTime:hh\\:mm}, HourMatch: {hoursMatch}, MinMatch: {minutesMatch})";
            SetResult(success, debugInfo);
        }
        else
        {
            SetResult(success);
        }
    }

    private void SetResult(bool success, string? debugInfo = null)
    {
        ResultSuccess = success;
        var baseMessage = success 
            ? _localizationService.GetString("ResultCorrect") 
            : _localizationService.GetString("ResultIncorrect");
        
        // Append debug info for incorrect answers in debug builds
        #if DEBUG
        ResultMessage = baseMessage + (debugInfo ?? string.Empty);
        #else
        ResultMessage = baseMessage;
        #endif
        
        ResultVisible = true;

        // Increment score on correct answer
        if (success)
        {
            CorrectAnswers++;
        }

        // Play audio feedback
        _ = success ? _audioService.PlaySuccessSound() : _audioService.PlayErrorSound();
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

    private void UpdateCultureDependentData()
    {
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(LanguageLabel));
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
        var minute = _random.Next(0, 12) * 5;  // 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55
        return new TimeSpan(hour, minute, 0);  // Seconds always 0
    }

    private bool TryParseUserTime(out TimeSpan time)
    {
        var input = AnswerText?.Trim();
        if (string.IsNullOrEmpty(input))
        {
            time = default;
            return false;
        }

        // Try digital formats first (HH:mm, H:mm, h:mm)
        var formats = new[] { "h\\:mm", "hh\\:mm", "H\\:mm", "HH\\:mm" };
        foreach (var format in formats)
        {
            if (TimeSpan.TryParseExact(input, format, CultureInfo.InvariantCulture, out time))
            {
                return true;
            }
        }

        // Try natural language based on current culture
        var isDutch = _localizationService.CurrentCulture.TwoLetterISOLanguageName.Equals("nl", StringComparison.OrdinalIgnoreCase);
        
        var parsedTime = isDutch 
            ? _dutchTimeParser.Parse(input)
            : _englishTimeParser.Parse(input);

        if (parsedTime.HasValue)
        {
            time = parsedTime.Value;
            return true;
        }

        time = default;
        return false;
    }

    private static bool MatchesTime(TimeSpan candidate, TimeSpan target)
    {
        // Normalize both times to 12-hour format using (hours % 12) + 12
        // This keeps values in 12-24 range, avoiding confusing 0 values
        // Examples: 0 ? 12, 1 ? 13, 5 ? 17, 12 ? 12, 13 ? 13, 17 ? 17
        var candidateHours = (candidate.Hours % 12) + 12;
        var targetHours = (target.Hours % 12) + 12;
        
        var candidateMinutes = candidate.Minutes;
        var targetMinutes = target.Minutes;
        
        // Check if hours match (same hour on 12-hour clock)
        var hoursMatch = candidateHours == targetHours;
        
        // Check if minutes are within 1 minute tolerance
        var minuteDiff = Math.Abs(candidateMinutes - targetMinutes);
        var minutesMatch = minuteDiff <= 1;
        
        return hoursMatch && minutesMatch;
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

    private double GetTargetMinuteValue()
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

    private void StartSecondTimer()
    {
        // Start with current second
        _currentSecond = DateTime.Now.Second;
        
        // Create a timer that ticks every second
        _secondTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_secondTimer != null)
        {
            _secondTimer.Interval = TimeSpan.FromSeconds(1);
            _secondTimer.Tick += (s, e) =>
            {
                _currentSecond = (_currentSecond + 1) % 60;
                OnPropertyChanged(nameof(SecondPointerValue));
            };
            _secondTimer.Start();
        }
    }
}
