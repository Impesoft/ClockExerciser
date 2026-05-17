using Clock_Exerciser.Core.Models;
using Clock_Exerciser.Core.Services;
using Clock_Exerciser.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Clock_Exerciser.Shared.Pages;

public partial class Game : IAsyncDisposable
{
    private GameMode? _loadedMode;
    private AnswerInput? answerInput;
    private IJSObjectReference? _keyboardHelperModule;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _keyboardHelperModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/Clock_Exerciser.Shared/keyboardHelper.js");
    }

    public async ValueTask DisposeAsync()
    {
        State.StopClock();
        State.StateChanged -= OnStateChanged;

        if (_keyboardHelperModule is not null)
        {
            try
            {
                await _keyboardHelperModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }

    private async Task OnPrimaryActionAsync()
    {
        try
        {
            if (_keyboardHelperModule is null)
            {
                _keyboardHelperModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./_content/Clock_Exerciser.Shared/keyboardHelper.js");
            }

            await _keyboardHelperModule.InvokeVoidAsync("dismissKeyboard");
        }
        catch
        {
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
