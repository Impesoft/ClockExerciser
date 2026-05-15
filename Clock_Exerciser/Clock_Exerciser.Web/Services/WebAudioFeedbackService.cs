using Clock_Exerciser.Core.Abstractions;

namespace Clock_Exerciser.Web.Services;

public sealed class WebAudioFeedbackService : IAudioFeedbackService
{
    public Task PlaySuccessSoundAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task PlayErrorSoundAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
