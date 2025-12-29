namespace ClockExerciser.Models;

/// <summary>
/// Represents a voice locale/accent option for text-to-speech
/// </summary>
public class LocaleInfo
{
    /// <summary>
    /// Full locale code (e.g., "en-US", "nl-BE")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Two-letter language code (e.g., "en", "nl")
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Two-letter country code (e.g., "US", "BE")
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly display name with flag emoji (e.g., "???? American English")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
