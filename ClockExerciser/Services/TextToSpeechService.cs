using ClockExerciser.Models;

namespace ClockExerciser.Services;

/// <summary>
/// Cross-platform text-to-speech implementation using MAUI's built-in TextToSpeech API
/// </summary>
public class TextToSpeechService : ITextToSpeechService
{
    public async Task SpeakAsync(string text, string? localeCode = null)
    {
        try
        {
            var options = new SpeechOptions();

            if (!string.IsNullOrEmpty(localeCode))
            {
                var locales = await TextToSpeech.Default.GetLocalesAsync();

                // Parse localeCode (e.g., "nl-BE")
                var parts = localeCode.Split('-');
                var language = parts[0];
                var country = parts.Length > 1 ? parts[1] : null;

                // Find exact match (language + country)
                var locale = locales.FirstOrDefault(l =>
                    l.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                    (country == null || l.Country.Equals(country, StringComparison.OrdinalIgnoreCase)));

                // Fallback to language-only match
                if (locale == null)
                {
                    locale = locales.FirstOrDefault(l =>
                        l.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
                }

                if (locale != null)
                {
                    options.Locale = locale;
                }
            }

            // Use CancellationToken with timeout to prevent hanging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await TextToSpeech.Default.SpeakAsync(text, options, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // TTS timed out - ignore
        }
        catch
        {
            // Silently fail - don't disrupt app if TTS fails
        }
    }

    public async Task<IEnumerable<LocaleInfo>> GetAvailableLocalesAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();

        // Filter to supported languages (English and Dutch) and map to LocaleInfo
        return locales
            .Where(l => l.Language == "en" || l.Language == "nl")
            .Select(l => new LocaleInfo
            {
                Code = $"{l.Language}-{l.Country}",
                Language = l.Language,
                Country = l.Country,
                DisplayName = GetDisplayName(l.Language, l.Country)
            })
            .ToList();
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            return locales?.Any() ?? false;
        }
        catch
        {
            return false;
        }
    }

    public void Stop()
    {
        // Note: MAUI's TextToSpeech API doesn't currently have a Stop method
        // If needed, this could be implemented with platform-specific code
        // For now, this is a no-op
    }

    private string GetDisplayName(string language, string country)
    {
        return (language, country) switch
        {
            ("en", "US") => "???? American English",
            ("en", "GB") => "???? British English",
            ("nl", "NL") => "???? Nederlands",
            ("nl", "BE") => "???? Vlaams",
            _ => $"{language}-{country}"
        };
    }
}
