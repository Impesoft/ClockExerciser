namespace Clock_Exerciser.Core.Abstractions;

public interface IAudioFeedbackService
{
    Task PlaySuccessSoundAsync(CancellationToken cancellationToken = default);

    Task PlayErrorSoundAsync(CancellationToken cancellationToken = default);
}
