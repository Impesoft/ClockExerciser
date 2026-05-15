using Clock_Exerciser.Core.Abstractions;

namespace Clock_Exerciser.Services;

public sealed class MauiAudioFeedbackService : IAudioFeedbackService
{
    public Task PlaySuccessSoundAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task PlayErrorSoundAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
