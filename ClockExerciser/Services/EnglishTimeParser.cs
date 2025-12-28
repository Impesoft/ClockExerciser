using System.Text.RegularExpressions;

namespace ClockExerciser.Services;

/// <summary>
/// Parses English natural language time expressions
/// Examples: "quarter past three", "ten to four", "half past two"
/// </summary>
public partial class EnglishTimeParser
{
    private static readonly Dictionary<string, int> HourWords = new()
    {
        {"one", 1}, {"two", 2}, {"three", 3}, {"four", 4},
        {"five", 5}, {"six", 6}, {"seven", 7}, {"eight", 8},
        {"nine", 9}, {"ten", 10}, {"eleven", 11}, {"twelve", 12}
    };

    private static readonly Dictionary<string, int> MinuteWords = new()
    {
        {"five", 5}, {"ten", 10}, {"quarter", 15}, {"twenty", 20}, {"twenty-five", 25}, {"half", 30}
    };

    /// <summary>
    /// Attempts to parse an English time expression
    /// </summary>
    /// <param name="input">English time string (e.g., "quarter past three")</param>
    /// <returns>TimeSpan if successful, null otherwise</returns>
    public TimeSpan? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim().ToLowerInvariant();

        // "quarter past [hour]" ? :15
        if (TryParseQuarterPast(input, out var quarterPastTime))
            return quarterPastTime;

        // "quarter to/before/of [hour]" ? :45
        if (TryParseQuarterTo(input, out var quarterToTime))
            return quarterToTime;

        // "half past [hour]" ? :30
        if (TryParseHalfPast(input, out var halfPastTime))
            return halfPastTime;

        // "[minutes] past [hour]" ? hour:minutes
        if (TryParseMinutesPast(input, out var pastTime))
            return pastTime;

        // "[minutes] to/before/of [hour]" ? (hour-1):(60-minutes)
        if (TryParseMinutesTo(input, out var toTime))
            return toTime;

        // "[hour] o'clock" ? hour:00
        if (TryParseOClock(input, out var oclockTime))
            return oclockTime;

        return null;
    }

    private bool TryParseQuarterPast(string input, out TimeSpan time)
    {
        var match = QuarterPastRegex().Match(input);
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
        {
            time = new TimeSpan(hour, 15, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseQuarterTo(string input, out TimeSpan time)
    {
        var match = QuarterToRegex().Match(input);
        if (match.Success && TryParseHourWord(match.Groups[1].Value, out var hour))
        {
            var actualHour = hour == 1 ? 12 : hour - 1;
            time = new TimeSpan(actualHour, 45, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseHalfPast(string input, out TimeSpan time)
    {
        var match = HalfPastRegex().Match(input);
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
        {
            time = new TimeSpan(hour, 30, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseMinutesPast(string input, out TimeSpan time)
    {
        var match = MinutesPastRegex().Match(input);
        if (match.Success)
        {
            var minuteStr = match.Groups[1].Value;
            var hourStr = match.Groups[2].Value;

            if (TryParseMinuteWord(minuteStr, out var minutes) &&
                TryParseHourWord(hourStr, out var hour))
            {
                time = new TimeSpan(hour, minutes, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private bool TryParseMinutesTo(string input, out TimeSpan time)
    {
        var match = MinutesToRegex().Match(input);
        if (match.Success)
        {
            var minuteStr = match.Groups[1].Value;
            var hourStr = match.Groups[2].Value;

            if (TryParseMinuteWord(minuteStr, out var minutes) &&
                TryParseHourWord(hourStr, out var hour))
            {
                var actualHour = hour == 1 ? 12 : hour - 1;
                time = new TimeSpan(actualHour, 60 - minutes, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private bool TryParseOClock(string input, out TimeSpan time)
    {
        var match = OClockRegex().Match(input);
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
        {
            time = new TimeSpan(hour, 0, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseMinuteWord(string word, out int minutes)
    {
        // Try predefined minute words first
        if (MinuteWords.TryGetValue(word, out minutes))
            return true;

        // Try numeric form
        if (int.TryParse(word, out minutes))
            return minutes >= 0 && minutes < 60;

        // Handle hyphenated forms like "twenty-five"
        if (word.Contains('-'))
        {
            var parts = word.Split('-');
            if (parts.Length == 2)
            {
                var tens = parts[0] == "twenty" ? 20 : 0;
                var ones = HourWords.TryGetValue(parts[1], out var val) ? val : 0;
                if (tens > 0 && ones < 10)
                {
                    minutes = tens + ones;
                    return true;
                }
            }
        }

        minutes = 0;
        return false;
    }

    private bool TryParseHourWord(string word, out int hour)
    {
        // Try predefined hour words first
        if (HourWords.TryGetValue(word, out hour))
            return true;

        // Try numeric form (1-12)
        if (int.TryParse(word, out hour))
            return hour >= 1 && hour <= 12;

        hour = 0;
        return false;
    }

    [GeneratedRegex(@"quarter past (\w+)")]
    private static partial Regex QuarterPastRegex();

    [GeneratedRegex(@"quarter (?:to|before|of) (\w+)")]
    private static partial Regex QuarterToRegex();

    [GeneratedRegex(@"half past (\w+)")]
    private static partial Regex HalfPastRegex();

    [GeneratedRegex(@"([\w-]+) past (\w+)")]
    private static partial Regex MinutesPastRegex();

    [GeneratedRegex(@"([\w-]+) (?:to|before|of) (\w+)")]
    private static partial Regex MinutesToRegex();

    [GeneratedRegex(@"(\w+) o'?clock")]
    private static partial Regex OClockRegex();
}
