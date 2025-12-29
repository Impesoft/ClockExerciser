namespace ClockExerciser.Services;

/// <summary>
/// Service for speech-to-text functionality using platform-specific APIs
/// </summary>
public interface ISpeechRecognitionService
{
    /// <summary>
    /// Starts listening and recognizes speech in the specified locale
    /// </summary>
    /// <param name="locale">Locale code (e.g., "en-US", "nl-NL")</param>
    /// <returns>Recognized text, or null if recognition failed or was cancelled</returns>
    Task<string?> RecognizeAsync(string locale = "en-US");

    /// <summary>
    /// Requests microphone permission from the user
    /// </summary>
    /// <returns>True if permission is granted, false otherwise</returns>
    Task<bool> RequestPermissionsAsync();

    /// <summary>
    /// Checks if speech recognition is available on this device
    /// </summary>
    Task<bool> IsAvailableAsync();
}
