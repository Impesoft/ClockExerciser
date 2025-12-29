namespace ClockExerciser.Services;

/// <summary>
/// Implementation of settings service using MAUI Preferences API for persistent storage
/// </summary>
public class SettingsService : ISettingsService
{
    // Preference keys
    private const string KEY_VOICE_OUTPUT = "VoiceOutputEnabled";
    private const string KEY_VOICE_INPUT = "VoiceInputEnabled";
    private const string KEY_PREFERRED_LOCALE_PREFIX = "PreferredLocale_";

    // Default values
    private const bool DEFAULT_VOICE_OUTPUT = true;
    private const bool DEFAULT_VOICE_INPUT = true;
    private const string DEFAULT_LOCALE_EN = "en-US";
    private const string DEFAULT_LOCALE_NL = "nl-NL";

    public Task<bool> GetVoiceOutputEnabledAsync()
    {
        var enabled = Preferences.Get(KEY_VOICE_OUTPUT, DEFAULT_VOICE_OUTPUT);
        return Task.FromResult(enabled);
    }

    public Task SetVoiceOutputEnabledAsync(bool enabled)
    {
        Preferences.Set(KEY_VOICE_OUTPUT, enabled);
        return Task.CompletedTask;
    }

    public Task<bool> GetVoiceInputEnabledAsync()
    {
        var enabled = Preferences.Get(KEY_VOICE_INPUT, DEFAULT_VOICE_INPUT);
        return Task.FromResult(enabled);
    }

    public Task SetVoiceInputEnabledAsync(bool enabled)
    {
        Preferences.Set(KEY_VOICE_INPUT, enabled);
        return Task.CompletedTask;
    }

    public Task<string> GetPreferredLocaleAsync(string language)
    {
        var key = KEY_PREFERRED_LOCALE_PREFIX + language;
        var defaultLocale = language switch
        {
            "en" => DEFAULT_LOCALE_EN,
            "nl" => DEFAULT_LOCALE_NL,
            _ => $"{language}-{language.ToUpper()}"
        };

        var locale = Preferences.Get(key, defaultLocale);
        return Task.FromResult(locale);
    }

    public Task SetPreferredLocaleAsync(string language, string localeCode)
    {
        var key = KEY_PREFERRED_LOCALE_PREFIX + language;
        Preferences.Set(key, localeCode);
        return Task.CompletedTask;
    }
}
