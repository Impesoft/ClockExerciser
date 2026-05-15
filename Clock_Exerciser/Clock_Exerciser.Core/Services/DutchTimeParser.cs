using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Clock_Exerciser.Core.Services;

public sealed partial class DutchTimeParser
{
    private static readonly Dictionary<string, int> HourWords = new()
    {
        ["een"] = 1,
        ["twee"] = 2,
        ["drie"] = 3,
        ["vier"] = 4,
        ["vijf"] = 5,
        ["zes"] = 6,
        ["zeven"] = 7,
        ["acht"] = 8,
        ["negen"] = 9,
        ["tien"] = 10,
        ["elf"] = 11,
        ["twaalf"] = 12
    };

    public TimeSpan? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // Normalize: remove accents, lowercase, trim
        input = NormalizeInput(input);

        if (TryParseKwartOver(input, out var kwartOverTime))
        {
            return kwartOverTime;
        }

        if (TryParseKwartVoor(input, out var kwartVoorTime))
        {
            return kwartVoorTime;
        }

        if (TryParseHalf(input, out var halfTime))
        {
            return halfTime;
        }

        if (TryParseMinutesOver(input, out var overTime))
        {
            return overTime;
        }

        if (TryParseMinutesVoor(input, out var voorTime))
        {
            return voorTime;
        }

        if (TryParseExactHour(input, out var hourTime))
        {
            return hourTime;
        }

        return null;
    }

    /// <summary>
    /// Normalize input: lowercase, remove accents (één → een), trim
    /// </summary>
    private static string NormalizeInput(string input)
    {
        input = input.Trim().ToLowerInvariant();

        // Remove diacritics/accents (één → een, ë → e, etc.)
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool TryParseKwartOver(string input, out TimeSpan time)
    {
        var match = KwartOverRegex().Match(input);
        if (match.Success && TryParseHourValue(match.Groups[1].Value, out var hour))
        {
            time = new TimeSpan(hour, 15, 0);
            return true;
        }

        time = default;
        return false;
    }

    private static bool TryParseKwartVoor(string input, out TimeSpan time)
    {
        var match = KwartVoorRegex().Match(input);
        if (match.Success && TryParseHourValue(match.Groups[1].Value, out var hour))
        {
            var actualHour = hour == 1 ? 12 : hour - 1;
            time = new TimeSpan(actualHour, 45, 0);
            return true;
        }

        time = default;
        return false;
    }

    private static bool TryParseHalf(string input, out TimeSpan time)
    {
        var match = HalfRegex().Match(input);
        if (match.Success && TryParseHourValue(match.Groups[1].Value, out var hour))
        {
            var actualHour = hour == 1 ? 12 : hour - 1;
            time = new TimeSpan(actualHour, 30, 0);
            return true;
        }

        time = default;
        return false;
    }

    private static bool TryParseMinutesOver(string input, out TimeSpan time)
    {
        var match = MinutesOverRegex().Match(input);
        if (match.Success)
        {
            var minuteText = match.Groups[1].Value;
            var hourText = match.Groups[2].Value;

            if (TryParseMinuteValue(minuteText, out var minutes) && TryParseHourValue(hourText, out var hour))
            {
                time = new TimeSpan(hour, minutes, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private static bool TryParseMinutesVoor(string input, out TimeSpan time)
    {
        var match = MinutesVoorRegex().Match(input);
        if (match.Success)
        {
            var minuteText = match.Groups[1].Value;
            var hourText = match.Groups[2].Value;

            if (TryParseMinuteValue(minuteText, out var minutes) && TryParseHourValue(hourText, out var hour))
            {
                var actualHour = hour == 1 ? 12 : hour - 1;
                time = new TimeSpan(actualHour, 60 - minutes, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private static bool TryParseExactHour(string input, out TimeSpan time)
    {
        var match = ExactHourRegex().Match(input);
        if (match.Success && TryParseHourValue(match.Groups[1].Value, out var hour))
        {
            time = new TimeSpan(hour, 0, 0);
            return true;
        }

        time = default;
        return false;
    }

    /// <summary>
    /// Parse hour value from either text (een, twee, ...) or number (1, 2, ...)
    /// </summary>
    private static bool TryParseHourValue(string text, out int hour)
    {
        // Try numeric first (e.g., "10")
        if (int.TryParse(text, out hour))
        {
            return hour >= 1 && hour <= 12;
        }

        // Try word lookup (e.g., "tien")
        return HourWords.TryGetValue(text, out hour);
    }

    /// <summary>
    /// Parse minute value from either text (vijf, tien, ...) or number (5, 10, ...)
    /// </summary>
    private static bool TryParseMinuteValue(string text, out int minutes)
    {
        // Try numeric first (e.g., "10")
        if (int.TryParse(text, out minutes))
        {
            return minutes >= 0 && minutes < 60;
        }

        // Try word lookup (e.g., "vijf" = 5) - only valid for minutes
        if (HourWords.TryGetValue(text, out minutes) && minutes <= 30)
        {
            return true;
        }

        minutes = 0;
        return false;
    }

    [GeneratedRegex(@"kwart over (\w+)")]
    private static partial Regex KwartOverRegex();

    [GeneratedRegex(@"kwart voor (\w+)")]
    private static partial Regex KwartVoorRegex();

    [GeneratedRegex(@"half (\w+)")]
    private static partial Regex HalfRegex();

    // Matches both "over" and "na" (5 over 10 OR 5 na 10)
    [GeneratedRegex(@"(\w+) (?:over|na) (\w+)")]
    private static partial Regex MinutesOverRegex();

    [GeneratedRegex(@"(\w+) voor (\w+)")]
    private static partial Regex MinutesVoorRegex();

    [GeneratedRegex(@"(\w+) uur")]
    private static partial Regex ExactHourRegex();
}

