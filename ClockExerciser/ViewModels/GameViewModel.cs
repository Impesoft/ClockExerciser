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

public sealed class GameViewModel : INotifyPropertyChanged, IQueryAttributable, IDisposable
{
    private readonly LocalizationService _localizationService;
    private readonly IAudioService _audioService;
    private readonly ISettingsService _settingsService;
    private readonly ITextToSpeechService _ttsService;
    private readonly ISpeechRecognitionService? _speechRecognitionService;
    private readonly DutchTimeParser _dutchTimeParser;
    private readonly EnglishTimeParser _englishTimeParser;
    private readonly IGameStateService _gameStateService;
    private readonly Random _random = new();
    private GameMode _selectedMode = GameMode.ClockToTime; // User's chosen mode (can be Random)
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
    private bool _isCheckingAnswer = false; // Prevents rapid-fire submissions

    public GameViewModel(LocalizationService localizationService, IAudioService audioService,
        ISettingsService settingsService, ITextToSpeechService ttsService,
        ISpeechRecognitionService? speechRecognitionService,
        DutchTimeParser dutchTimeParser, EnglishTimeParser englishTimeParser, IGameStateService gameStateService)
    {
        _localizationService = localizationService;
        _audioService = audioService;
        _settingsService = settingsService;
        _ttsService = ttsService;
        _speechRecognitionService = speechRecognitionService;
        _dutchTimeParser = dutchTimeParser;
        _englishTimeParser = englishTimeParser;
        _gameStateService = gameStateService;
        _localizationService.CultureChanged += (_, _) => OnCultureChanged();
        _gameStateService.GameStateChanged += (_, _) => OnGameStateChanged();

        Languages = new ObservableCollection<LanguageOption>
        {
            new("English", "en-US"),
            new("Nederlands", "nl-NL")
        };

        CheckAnswerCommand = new Command(ExecutePrimaryAction);
        NewGameCommand = new Command(StartNewGame);
        SwitchToClockToTimeCommand = new Command(() => SwitchMode(GameMode.ClockToTime));
        SwitchToTimeToClockCommand = new Command(() => SwitchMode(GameMode.TimeToClock));
        SwitchToRandomModeCommand = new Command(() => SwitchMode(GameMode.Random));
        MicrophoneCommand = new Command(async () => await UseSpeechRecognitionAsync());

        UpdateCultureDependentData();

        // Set selected language to match current culture from service
        SelectedLanguage = Languages.FirstOrDefault(l => l.Culture.Name == _localizationService.CurrentCulture.Name)
                          ?? Languages.First();
        
        // Start the second hand timer
        StartSecondTimer();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LanguageOption> Languages { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var modeObj) && modeObj is GameMode mode)
        {
            // Store the user's selected mode (this can be Random)
            _selectedMode = mode;
            
            // Only start new game if we're coming from menu (no existing game)
            // If there's already a game in progress, just switch mode and continue
            if (_gameStateService.CorrectAnswers == 0 && _gameStateService.WrongAnswers == 0)
            {
                StartNewGame();
            }
            else
            {
                // Just generate new challenge with the new mode
                GenerateNewChallenge();
            }
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

    public bool CanSubmit => !_isCheckingAnswer && (IsClockToTime ? !string.IsNullOrWhiteSpace(AnswerText) : true);

    public bool IsClockToTime => _gameStateService.ActiveMode == GameMode.ClockToTime;

    public bool IsTimeToClock => _gameStateService.ActiveMode == GameMode.TimeToClock;
    
    public int CorrectAnswers => _gameStateService.CorrectAnswers;
    
    public int WrongAnswers => _gameStateService.WrongAnswers;
    
    public int EffectiveScore => _gameStateService.EffectiveScore;
    
    public int HighScore => _gameStateService.HighScore;
    
    public bool IsGameOver => _gameStateService.IsGameOver;
    
    public int MaxWrongAnswers => _gameStateService.MaxWrongAnswers;
    
    public bool IsBeginnerMode => _gameStateService.CurrentDifficulty == Models.DifficultyLevel.Beginner;
    
    public string PrimaryButtonText => _localizationService.GetString("SubmitAnswer");
    
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

    public double HourPointerValue => IsClockToTime ? GetTargetHourPointer() : ConvertCenteredHourToAbsolute(UserHourValue);

    public double MinutePointerValue => ConvertToDialValue(IsClockToTime ? GetTargetMinuteValue() : ConvertCenteredMinuteToAbsolute(UserMinuteValue));

    public double SecondPointerValue => ConvertToDialValue(_currentSecond);  // Always show ticking second hand

    private static double ConvertToDialValue(double rawValue) => rawValue / 5d;
    
    private static double ConvertCenteredHourToAbsolute(double centeredValue)
    {
        // Convert from -12 to +12 range to 0 to 12 range
        // Left side (negative): goes from 12 down to 1 (12 - abs(value))
        // Right side (positive): goes from 1 up to 12 (value)
        // Center (0): represents 12
        if (centeredValue == 0)
            return 12;
        else if (centeredValue < 0)
            return 12 - Math.Abs(centeredValue); // Left: 12, 11, 10, ..., 1
        else
            return centeredValue; // Right: 1, 2, 3, ..., 12
    }
    
    private static double ConvertCenteredMinuteToAbsolute(double centeredValue)
    {
        // Convert from -60 to +60 range to 0 to 59 range
        // Left side (negative): goes from 60 down to 1 (60 - abs(value))
        // Right side (positive): goes from 1 up to 60 (value)
        // Center (0): represents 0
        if (centeredValue == 0)
            return 0;
        else if (centeredValue < 0)
            return 60 - Math.Abs(centeredValue); // Left: 60, 59, 58, ..., 1
        else
            return Math.Min(centeredValue, 59); // Right: 1, 2, 3, ..., 59
    }
    
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
    public string NewGameButtonText => _localizationService.GetString("NewGame");
    public string InstructionText => IsClockToTime ? _localizationService.GetString("ClockToTimeInstruction") : _localizationService.GetString("TimeToClockInstruction");

    public Command CheckAnswerCommand { get; }
    public Command NewGameCommand { get; }
    public Command SwitchToClockToTimeCommand { get; }
    public Command SwitchToTimeToClockCommand { get; }
    public Command SwitchToRandomModeCommand { get; }
    public Command MicrophoneCommand { get; }

    private void ExecutePrimaryAction()
    {
        // Don't allow checking answer if game is over or already checking
        if (IsGameOver || _isCheckingAnswer)
        {
            return;
        }
        
        // Disable button immediately to prevent rapid-fire submissions
        _isCheckingAnswer = true;
        OnPropertyChanged(nameof(CanSubmit));
        
        // User clicked "Check Answer"
        ExecuteCheckAnswer();
    }
    
    private void ExecuteCheckAnswer()
    {
        // Process the answer
        if (_gameStateService.ActiveMode == GameMode.ClockToTime)
        {
            EvaluateTextAnswer();
        }
        else
        {
            EvaluateClockAnswer();
        }
        
        // If answer was correct, automatically proceed to next challenge after a brief delay
        if (_resultSuccess)
        {
            // Show success message briefly, then auto-advance
            Task.Delay(1500).ContinueWith(_ => 
            {
                Application.Current?.Dispatcher.Dispatch(() => GenerateNewChallenge());
            });
        }
        else
        {
            // If answer was wrong, re-enable the button after a short delay
            Task.Delay(500).ContinueWith(_ => 
            {
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    _isCheckingAnswer = false;
                    OnPropertyChanged(nameof(CanSubmit));
                });
            });
        }
    }
    
    private void StartNewGame()
    {
        // Reset game state through service
        _gameStateService.ResetGame();
        ResultVisible = false;
        ResultMessage = string.Empty;
        ResultSuccess = false;
        
        // Generate first challenge
        GenerateNewChallenge();
    }
    
    private void SwitchMode(GameMode newMode)
    {
        // Don't switch mode if game is over
        if (IsGameOver)
        {
            return;
        }
        
        // Update the selected mode
        _selectedMode = newMode;
        
        // Generate new challenge with new mode
        GenerateNewChallenge();
    }

    public void GenerateNewChallenge()
    {
        // Don't generate new challenge if game is over
        if (IsGameOver)
        {
            return;
        }
        
        // Determine actual mode for this challenge
        // If user selected Random, pick randomly; otherwise use their selected mode
        var challengeMode = _selectedMode == GameMode.Random 
            ? (_random.Next(0, 2) == 0 ? GameMode.ClockToTime : GameMode.TimeToClock)
            : _selectedMode;
        
        // Update the active mode in service for this challenge
        _gameStateService.ActiveMode = challengeMode;
        
        TargetTime = CreateRandomTime();
        AnswerText = string.Empty;
        ResultVisible = false;
        ResultMessage = string.Empty;
        ResultSuccess = false;
        
        // Re-enable the check button for the new challenge
        _isCheckingAnswer = false;
        
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
        
        // Speak the prompt if voice output is enabled
        SpeakPromptAsync();
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
        // Convert centered slider values to absolute clock values
        var userHourAbsolute = ConvertCenteredHourToAbsolute(UserHourValue);
        var userMinuteAbsolute = ConvertCenteredMinuteToAbsolute(UserMinuteValue);
        
        // Get integer hour from user (0-12)
        var userHour = (int)userHourAbsolute;
        // Get integer hour from target (0-11, then wrap 0 to 12)
        var targetHour = TargetTime.Hours % 12;
        if (targetHour == 0) targetHour = 12;
        
        // Get integer minute from user (0-59)
        var userMinute = (int)userMinuteAbsolute;
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

        // Update score based on result through service
        if (success)
        {
            _gameStateService.CorrectAnswers++;
        }
        else
        {
            _gameStateService.WrongAnswers++;
            
            // Check if game is over
            if (IsGameOver)
            {
                ResultMessage = string.Format(_localizationService.GetString("GameOver"), CorrectAnswers);
            }
        }

        // Play audio feedback
        _ = success ? _audioService.PlaySuccessSound() : _audioService.PlayErrorSound();
        
        // Speak feedback if voice output is enabled
        SpeakFeedbackAsync(success);
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
        OnPropertyChanged(nameof(NewGameButtonText));
        OnPropertyChanged(nameof(InstructionText));
    }

    private void OnCultureChanged()
    {
        UpdateCultureDependentData();
        UpdatePromptTexts();
    }

    private void OnGameStateChanged()
    {
        // Notify UI of score and game over state changes
        OnPropertyChanged(nameof(CorrectAnswers));
        OnPropertyChanged(nameof(WrongAnswers));
        OnPropertyChanged(nameof(EffectiveScore));
        OnPropertyChanged(nameof(HighScore));
        OnPropertyChanged(nameof(IsGameOver));
        OnPropertyChanged(nameof(MaxWrongAnswers));
        OnPropertyChanged(nameof(IsBeginnerMode));
        OnPropertyChanged(nameof(IsClockToTime));
        OnPropertyChanged(nameof(IsTimeToClock));
        OnPropertyChanged(nameof(InstructionText));
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
            1 => "één",
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
    
    private void SpeakPromptAsync()
    {
        // Fire-and-forget - don't block UI
        _ = Task.Run(async () =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? SpeakPromptAsync: Starting");
                
                // Check if voice output is enabled
                var voiceEnabled = await _settingsService.GetVoiceOutputEnabledAsync();
                System.Diagnostics.Debug.WriteLine($"?? Voice output enabled: {voiceEnabled}");
                
                if (!voiceEnabled)
                    return;

                // Get preferred locale for current language
                var currentLanguage = _localizationService.CurrentCulture.TwoLetterISOLanguageName;
                var preferredLocale = await _settingsService.GetPreferredLocaleAsync(currentLanguage);
                System.Diagnostics.Debug.WriteLine($"?? Preferred locale: {preferredLocale}");

                // Determine what to speak based on mode
                string textToSpeak;
                if (IsClockToTime)
                {
                    // Clock to Time: "What time is shown on the clock?"
                    textToSpeak = InstructionText;
                }
                else
                {
                    // Time to Clock: "Set the clock to [time]"
                    textToSpeak = $"{InstructionText} {PromptText}";
                }

                System.Diagnostics.Debug.WriteLine($"?? About to speak: '{textToSpeak}'");
                await _ttsService.SpeakAsync(textToSpeak, preferredLocale);
                System.Diagnostics.Debug.WriteLine("?? SpeakPromptAsync: Completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? SpeakPromptAsync Error: {ex.Message}");
            }
        });
    }

    private void SpeakFeedbackAsync(bool success)
    {
        // Fire-and-forget - don't block UI
        _ = Task.Run(async () =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? SpeakFeedbackAsync: Starting");
                
                // Check if voice output is enabled
                var voiceEnabled = await _settingsService.GetVoiceOutputEnabledAsync();
                System.Diagnostics.Debug.WriteLine($"?? Voice output enabled: {voiceEnabled}");
                
                if (!voiceEnabled)
                    return;

                // Get preferred locale for current language
                var currentLanguage = _localizationService.CurrentCulture.TwoLetterISOLanguageName;
                var preferredLocale = await _settingsService.GetPreferredLocaleAsync(currentLanguage);
                System.Diagnostics.Debug.WriteLine($"?? Preferred locale: {preferredLocale}");

                // Speak appropriate feedback
                string textToSpeak;
                if (IsGameOver)
                {
                    // Game over: speak final score
                    textToSpeak = string.Format(
                        _localizationService.GetString("TTSScore"),
                        CorrectAnswers
                    );
                }
                else if (success)
                {
                    textToSpeak = _localizationService.GetString("TTSCorrect");
                }
                else
                {
                    textToSpeak = _localizationService.GetString("TTSIncorrect");
                }

                System.Diagnostics.Debug.WriteLine($"?? About to speak feedback: '{textToSpeak}'");
                await _ttsService.SpeakAsync(textToSpeak, preferredLocale);
                System.Diagnostics.Debug.WriteLine("?? SpeakFeedbackAsync: Completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? SpeakFeedbackAsync Error: {ex.Message}");
            }
        });
    }

    private async Task UseSpeechRecognitionAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("?? UseSpeechRecognitionAsync: Starting");

            // Check if speech recognition service is available
            if (_speechRecognitionService == null)
            {
                System.Diagnostics.Debug.WriteLine("?? Speech recognition service not available on this platform");
                return;
            }

            // Check if voice input is enabled
            var voiceInputEnabled = await _settingsService.GetVoiceInputEnabledAsync();
            System.Diagnostics.Debug.WriteLine($"?? Voice input enabled: {voiceInputEnabled}");
            
            if (!voiceInputEnabled)
                return;

            // Check if available
            if (!await _speechRecognitionService.IsAvailableAsync())
            {
                System.Diagnostics.Debug.WriteLine("?? Speech recognition not available on device");
                return;
            }

            // Get current locale
            var currentLanguage = _localizationService.CurrentCulture.TwoLetterISOLanguageName;
            var preferredLocale = await _settingsService.GetPreferredLocaleAsync(currentLanguage);
            System.Diagnostics.Debug.WriteLine($"?? Using locale: {preferredLocale}");

            // Start recognition
            var recognizedText = await _speechRecognitionService.RecognizeAsync(preferredLocale);
            System.Diagnostics.Debug.WriteLine($"?? Recognized text: '{recognizedText}'");

            if (!string.IsNullOrWhiteSpace(recognizedText))
            {
                // Set the recognized text as the answer
                AnswerText = recognizedText;
                System.Diagnostics.Debug.WriteLine($"?? Answer text set to: '{AnswerText}'");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("?? No text recognized or recognition cancelled");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"?? UseSpeechRecognitionAsync Error: {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        if (_secondTimer != null)
        {
            _secondTimer.Stop();
            _secondTimer = null;
        }
    }
}
