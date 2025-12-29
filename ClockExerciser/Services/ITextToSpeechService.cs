using ClockExerciser.Models;

namespace ClockExerciser.Services;

/// <summary>
/// Service for text-to-speech functionality using MAUI's built-in API
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>
    /// Speaks the given text using the specified locale
    /// </summary>
    /// <param name="text">Text to speak</param>
    /// <param name="localeCode">Optional locale code (e.g., "en-US", "nl-BE"). If null, uses system default.</param>
    Task SpeakAsync(string text, string? localeCode = null);

    /// <summary>
    /// Gets all available voice locales for supported languages (English and Dutch)
    /// </summary>
    Task<IEnumerable<LocaleInfo>> GetAvailableLocalesAsync();

    /// <summary>
    /// Checks if text-to-speech is available on this device
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Stops any currently playing speech
    /// </summary>
    void Stop();
}
