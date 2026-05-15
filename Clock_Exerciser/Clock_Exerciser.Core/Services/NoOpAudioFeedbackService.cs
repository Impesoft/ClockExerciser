using Clock_Exerciser.Core.Abstractions;

namespace Clock_Exerciser.Core.Services;

public sealed class NoOpAudioFeedbackService : IAudioFeedbackService
{
    public Task PlaySuccessSoundAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task PlayErrorSoundAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
