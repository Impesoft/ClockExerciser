using Clock_Exerciser.Core.Models;
using Clock_Exerciser.Core.Services;
using Clock_Exerciser.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Clock_Exerciser.Shared.Pages;

public partial class Game : IDisposable
{
    private GameMode? _loadedMode;
    private AnswerInput? answerInput;
    private ElementReference submitButton;

    [Inject]
    private ClockExerciseState State { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public string? ModeName { get; set; }

    private string CurrentLanguageName => State.GetCurrentLanguage().CultureName;

    protected override async Task OnInitializedAsync()
    {
        State.StateChanged += OnStateChanged;
        await State.InitializeAsync();
        State.StartClock();
    }

    protected override void OnParametersSet()
    {
        var mode = ParseMode(ModeName);
        if (_loadedMode != mode)
        {
            _loadedMode = mode;
            State.GenerateNewChallenge(mode);
        }
    }

    public void Dispose()
    {
        State.StopClock();
        State.StateChanged -= OnStateChanged;
    }

    private async Task OnPrimaryActionAsync()
    {
        // Focus the button to dismiss keyboard (Android will auto-hide keyboard when input loses focus)
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", "arguments[0].focus()", submitButton);
        }
        catch
        {
            // Silently fail - not critical
        }

        await State.ExecutePrimaryActionAsync();
    }

    private void NavigateToMenu()
    {
        NavigationManager.NavigateTo("/");
    }

    private void OnAnswerInput(string value)
    {
        State.AnswerText = value;
    }

    private void OnHourChanged(double value)
    {
        State.UserHourValue = value;
    }

    private void OnMinuteChanged(double value)
    {
        State.UserMinuteValue = value;
    }

    private async Task OnLanguageChangedAsync(string cultureName)
    {
        var language = State.Languages.FirstOrDefault(option => option.CultureName == cultureName);
        if (language is not null)
        {
            await State.SetCultureAsync(language);
        }
    }

    private void OnStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private static GameMode ParseMode(string? modeName)
    {
        return Enum.TryParse<GameMode>(modeName, ignoreCase: true, out var mode) ? mode : GameMode.ClockToTime;
    }
}
