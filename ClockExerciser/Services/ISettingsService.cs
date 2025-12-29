namespace ClockExerciser.Services;

/// <summary>
/// Service for managing application settings and user preferences
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets whether voice output (text-to-speech) is enabled
    /// </summary>
    Task<bool> GetVoiceOutputEnabledAsync();

    /// <summary>
    /// Sets whether voice output (text-to-speech) is enabled
    /// </summary>
    Task SetVoiceOutputEnabledAsync(bool enabled);

    /// <summary>
    /// Gets whether voice input (speech recognition) is enabled
    /// </summary>
    Task<bool> GetVoiceInputEnabledAsync();

    /// <summary>
    /// Sets whether voice input (speech recognition) is enabled
    /// </summary>
    Task SetVoiceInputEnabledAsync(bool enabled);

    /// <summary>
    /// Gets the preferred locale code for a given language (e.g., "en-US" for English)
    /// </summary>
    /// <param name="language">Two-letter language code (e.g., "en", "nl")</param>
    /// <returns>Locale code (e.g., "en-US", "nl-BE")</returns>
    Task<string> GetPreferredLocaleAsync(string language);

    /// <summary>
    /// Sets the preferred locale code for a given language
    /// </summary>
    /// <param name="language">Two-letter language code (e.g., "en", "nl")</param>
    /// <param name="localeCode">Locale code (e.g., "en-US", "nl-BE")</param>
    Task SetPreferredLocaleAsync(string language, string localeCode);
}
