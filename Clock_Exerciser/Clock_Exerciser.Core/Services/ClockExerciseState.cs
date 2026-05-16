using System.Globalization;
using Clock_Exerciser.Core.Abstractions;
using Clock_Exerciser.Core.Models;

namespace Clock_Exerciser.Core.Services;

public sealed class ClockExerciseState : IAsyncDisposable
{
    private const string CorrectAnswersPreferenceKey = "CorrectAnswers";
    private const string ErrorCountPreferenceKey = "ErrorCount";
    private readonly ChallengeGenerator _challengeGenerator;
    private readonly UserTimeParser _userTimeParser;
    private readonly AnswerValidator _answerValidator;
    private readonly ITextProvider _textProvider;
    private readonly ICultureStore _cultureStore;
    private readonly IPreferenceStore _preferenceStore;
    private readonly IAudioFeedbackService _audioFeedbackService;
    private readonly IClockTicker _clockTicker;
    private GameMode _requestedMode = GameMode.ClockToTime;
    private GameMode _activeMode = GameMode.ClockToTime;
    private TimeSpan _targetTime;
    private string _answerText = string.Empty;
    private string _resultMessage = string.Empty;
    private bool _resultVisible;
    private bool _resultSuccess;
    private double _userHourValue;
    private double _userMinuteValue;
    private int _correctAnswers;
    private int _errorCount;
    private bool _answerChecked;
    private bool _initialized;
    private System.Timers.Timer? _countdownTimer;
    private int _countdown;

    public ClockExerciseState(
        ChallengeGenerator challengeGenerator,
        UserTimeParser userTimeParser,
        AnswerValidator answerValidator,
        ITextProvider textProvider,
        ICultureStore cultureStore,
        IPreferenceStore preferenceStore,
        IAudioFeedbackService audioFeedbackService,
        IClockTicker clockTicker)
    {
        _challengeGenerator = challengeGenerator;
        _userTimeParser = userTimeParser;
        _answerValidator = answerValidator;
        _textProvider = textProvider;
        _cultureStore = cultureStore;
        _preferenceStore = preferenceStore;
        _audioFeedbackService = audioFeedbackService;
        _clockTicker = clockTicker;
        _cultureStore.CultureChanged += OnCultureChanged;
        _clockTicker.SecondChanged += OnSecondChanged;
    }

    public event Action? StateChanged;

    public IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("English", "en-US"),
        new("Nederlands", "nl-NL")
    ];

    public CultureInfo CurrentCulture => _cultureStore.CurrentCulture;

    public GameMode RequestedMode => _requestedMode;

    public GameMode ActiveMode => _activeMode;

    public bool IsClockToTime => _activeMode == GameMode.ClockToTime;

    public bool IsTimeToClock => _activeMode == GameMode.TimeToClock;

    public TimeSpan TargetTime => _targetTime;

    public string AnswerText
    {
        get => _answerText;
        set
        {
            if (_answerText == value)
            {
                return;
            }

            _answerText = value;
            NotifyStateChanged();
        }
    }

    public string PromptDigital => TargetTime.ToString("hh\\:mm", CultureInfo.InvariantCulture);

    public string PromptText => TimePhraseFormatter.FormatFriendlyTime(TargetTime, CurrentCulture);

    public string ResultMessage => _resultMessage;

    public bool ResultVisible => _resultVisible;

    public bool ResultSuccess => _resultSuccess;

    public bool CanSubmit => IsTimeToClock || !string.IsNullOrWhiteSpace(AnswerText);

    public int CorrectAnswers => _correctAnswers;

    public int ErrorCount => _errorCount;

    public string ScoreText => ScoreFormatter.Format(CorrectAnswers, ErrorCount);

    public int Countdown => _countdown;

    public bool IsCountdownActive => _countdown > 0;

    public string PrimaryButtonText
    {
        get
        {
            if (_answerChecked && _resultSuccess)
            {
                return IsCountdownActive 
                    ? $"{Text(AppTextKeys.NextChallenge)} ({_countdown})" 
                    : Text(AppTextKeys.NextChallenge);
            }
            return Text(AppTextKeys.SubmitAnswer);
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
            NotifyStateChanged();
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
            NotifyStateChanged();
        }
    }

    public int CurrentSecond => _clockTicker.CurrentSecond;

    public double HourPointerValue => IsClockToTime 
        ? ClockMath.GetHourPointerValue(TargetTime) 
        : UserHourValue + (UserMinuteValue / 60d);

    public double MinutePointerValue => IsClockToTime ? ClockMath.GetMinutePointerValue(TargetTime) : ClockMath.ConvertToDialValue(UserMinuteValue);

    public double SecondPointerValue => ClockMath.GetSecondPointerValue(CurrentSecond);

    public string InstructionText => IsClockToTime
        ? Text(AppTextKeys.ClockToTimeInstruction)
        : Text(AppTextKeys.TimeToClockInstruction);

    public string Text(string key) => _textProvider.GetString(key);

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _correctAnswers = await _preferenceStore.GetIntAsync(CorrectAnswersPreferenceKey, 0, cancellationToken);
        _errorCount = await _preferenceStore.GetIntAsync(ErrorCountPreferenceKey, 0, cancellationToken);
        GenerateNewChallenge(GameMode.ClockToTime);
    }

    public async ValueTask SetCultureAsync(LanguageOption language, CancellationToken cancellationToken = default)
    {
        await _cultureStore.SetCultureAsync(language.Culture, cancellationToken);
    }

    public void GenerateNewChallenge(GameMode? requestedMode = null)
    {
        StopCountdown();

        if (requestedMode.HasValue)
        {
            _requestedMode = requestedMode.Value;
        }

        var challenge = _challengeGenerator.Create(_requestedMode);
        _activeMode = challenge.Mode;
        _targetTime = challenge.TargetTime;
        _answerText = string.Empty;
        _resultVisible = false;
        _resultSuccess = false;
        _resultMessage = string.Empty;
        _answerChecked = false;
        _countdown = 0;

        if (IsTimeToClock)
        {
            _userHourValue = 0;
            _userMinuteValue = 0;
        }

        NotifyStateChanged();
    }

    public async Task ExecutePrimaryActionAsync(CancellationToken cancellationToken = default)
    {
        if (_answerChecked && _resultSuccess)
        {
            GenerateNewChallenge();
            return;
        }

        await CheckAnswerAsync(cancellationToken);
    }

    public void StopCountdown()
    {
        if (_countdownTimer is not null)
        {
            _countdownTimer.Stop();
            _countdownTimer.Elapsed -= OnCountdownTick;
            _countdownTimer.Dispose();
            _countdownTimer = null;
        }
        _countdown = 0;
    }

    private void StartCountdown()
    {
        _countdown = 3;
        _countdownTimer = new System.Timers.Timer(1000);
        _countdownTimer.Elapsed += OnCountdownTick;
        _countdownTimer.Start();
        NotifyStateChanged();
    }

    private void OnCountdownTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _countdown--;

        if (_countdown <= 0)
        {
            StopCountdown();
            GenerateNewChallenge();
        }

        NotifyStateChanged();
    }

    public void StartClock() => _clockTicker.Start();

    public void StopClock() => _clockTicker.Stop();

    public LanguageOption GetCurrentLanguage()
    {
        return Languages.FirstOrDefault(language => language.CultureName.Equals(CurrentCulture.Name, StringComparison.OrdinalIgnoreCase))
            ?? Languages[0];
    }

    public async ValueTask DisposeAsync()
    {
        StopCountdown();
        _cultureStore.CultureChanged -= OnCultureChanged;
        _clockTicker.SecondChanged -= OnSecondChanged;
        await _clockTicker.DisposeAsync();
    }

    private async Task CheckAnswerAsync(CancellationToken cancellationToken)
    {
        var success = IsClockToTime ? EvaluateTextAnswer() : EvaluateClockAnswer();
        await SetResultAsync(success, cancellationToken);
        _answerChecked = true;
        NotifyStateChanged();
    }

    private bool EvaluateTextAnswer()
    {
        if (!_userTimeParser.TryParse(AnswerText, CurrentCulture, out var userTime))
        {
            return false;
        }

        return _answerValidator.EvaluateTextAnswer(userTime, TargetTime);
    }

    private bool EvaluateClockAnswer()
    {
        return _answerValidator.EvaluateClockAnswer(UserHourValue, UserMinuteValue, TargetTime);
    }

    private async Task SetResultAsync(bool success, CancellationToken cancellationToken)
    {
        _resultSuccess = success;
        _resultMessage = success ? Text(AppTextKeys.ResultCorrect) : Text(AppTextKeys.ResultIncorrect);
        _resultVisible = true;

        if (success)
        {
            _correctAnswers++;
            await _preferenceStore.SetIntAsync(CorrectAnswersPreferenceKey, _correctAnswers, cancellationToken);
            await _audioFeedbackService.PlaySuccessSoundAsync(cancellationToken);
            StartCountdown();
        }
        else
        {
            _errorCount++;
            await _preferenceStore.SetIntAsync(ErrorCountPreferenceKey, _errorCount, cancellationToken);
            await _audioFeedbackService.PlayErrorSoundAsync(cancellationToken);
        }
    }

    private void OnCultureChanged(object? sender, CultureInfo culture)
    {
        NotifyStateChanged();
    }

    private void OnSecondChanged(object? sender, int second)
    {
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
