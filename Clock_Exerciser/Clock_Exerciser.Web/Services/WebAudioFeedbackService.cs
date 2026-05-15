using Clock_Exerciser.Core.Abstractions;
using Microsoft.JSInterop;

namespace Clock_Exerciser.Web.Services;

public sealed class WebAudioFeedbackService : IAudioFeedbackService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public WebAudioFeedbackService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            jsRuntime.InvokeAsync<IJSObjectReference>("import", "./audioPlayer.js").AsTask());
    }

    public async Task PlaySuccessSoundAsync(CancellationToken cancellationToken = default)
    {
        await PlaySoundAsync("success.mp3", cancellationToken);
    }

    public async Task PlayErrorSoundAsync(CancellationToken cancellationToken = default)
    {
        await PlaySoundAsync("error.mp3", cancellationToken);
    }

    private async Task PlaySoundAsync(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("playAudio", cancellationToken, $"sounds/{fileName}");
        }
        catch (Exception ex)
        {
            // Log error but don't crash - audio is not critical
            Console.WriteLine($"Audio playback failed: {ex.Message}");
        }
    }
}

