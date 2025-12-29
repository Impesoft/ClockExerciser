namespace ClockExerciser.Models;

/// <summary>
/// Represents a regional accent option for the Settings page picker
/// </summary>
public class RegionOption
{
    /// <summary>
    /// Two-letter country code (e.g., "US", "GB", "NL", "BE")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full locale code (e.g., "en-US", "nl-BE")
    /// </summary>
    public string LocaleCode { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly display name with flag emoji (e.g., "???? American", "???? Vlaams")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
