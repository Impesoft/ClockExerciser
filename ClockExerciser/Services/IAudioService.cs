namespace ClockExerciser.Services;

/// <summary>
/// Service for playing audio feedback sounds during gameplay
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Plays a happy, encouraging sound when the user answers correctly
    /// </summary>
    Task PlaySuccessSound();

    /// <summary>
    /// Plays a gentle "try again" sound when the user answers incorrectly
    /// </summary>
    Task PlayErrorSound();
}
