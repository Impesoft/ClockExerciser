using Clock_Exerciser.Core.Abstractions;
using Plugin.Maui.Audio;

namespace Clock_Exerciser.Services;

public sealed class MauiAudioFeedbackService : IAudioFeedbackService
{
    private readonly IAudioManager _audioManager;

    public MauiAudioFeedbackService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
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
            var audioStream = await FileSystem.OpenAppPackageFileAsync(fileName);
            var player = _audioManager.CreatePlayer(audioStream);
            player.Volume = 0.5; // 50% volume
            player.Play();
        }
        catch (Exception ex)
        {
            // Log error but don't crash - audio is not critical
            Console.WriteLine($"Audio playback failed: {ex.Message}");
        }
    }
}

