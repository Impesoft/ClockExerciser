using Clock_Exerciser.Core.Abstractions;
using Microsoft.JSInterop;

namespace Clock_Exerciser.Web.Client.Services;

public sealed class BrowserAudioFeedbackService : IAudioFeedbackService
{
    private const string AudioModulePath = "/_content/Clock_Exerciser.Shared/audioPlayer.js";
    private const string SharedSoundsBasePath = "/_content/Clock_Exerciser.Shared/sounds/";
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public BrowserAudioFeedbackService(IJSRuntime jsRuntime)
    {
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            jsRuntime.InvokeAsync<IJSObjectReference>("import", AudioModulePath).AsTask());
    }

    public Task PlaySuccessSoundAsync(CancellationToken cancellationToken = default)
        => PlaySoundAsync("success.mp3", cancellationToken);

    public Task PlayErrorSoundAsync(CancellationToken cancellationToken = default)
        => PlaySoundAsync("error.mp3", cancellationToken);

    private async Task PlaySoundAsync(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("playAudio", cancellationToken, $"{SharedSoundsBasePath}{fileName}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Audio playback failed: {ex}");
        }
    }
}
