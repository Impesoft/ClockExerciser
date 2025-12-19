using System.Text.RegularExpressions;

namespace ClockExerciser.Services;

/// <summary>
/// Parses Dutch natural language time expressions
/// Examples: "kwart over vijf", "half vijf", "tien voor vier"
/// </summary>
public partial class DutchTimeParser
{
    private static readonly Dictionary<string, int> HourWords = new()
    {
        {"een", 1}, {"twee", 2}, {"drie", 3}, {"vier", 4},
        {"vijf", 5}, {"zes", 6}, {"zeven", 7}, {"acht", 8},
        {"negen", 9}, {"tien", 10}, {"elf", 11}, {"twaalf", 12}
    };

    /// <summary>
    /// Attempts to parse a Dutch time expression
    /// </summary>
    /// <param name="input">Dutch time string (e.g., "kwart over vijf")</param>
    /// <returns>TimeSpan if successful, null otherwise</returns>
    public TimeSpan? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim().ToLowerInvariant();

        // "kwart over [uur]" ? :15
        if (TryParseKwartOver(input, out var kwartOverTime))
            return kwartOverTime;

        // "kwart voor [uur]" ? :45 (15 minutes before next hour)
        if (TryParseKwartVoor(input, out var kwartVoorTime))
            return kwartVoorTime;

        // "half [uur]" ? :30 (30 minutes before the hour)
        if (TryParseHalf(input, out var halfTime))
            return halfTime;

        // "[minuten] over [uur]" ? hour:minutes
        if (TryParseMinutesOver(input, out var overTime))
            return overTime;

        // "[minuten] voor [uur]" ? (hour-1):(60-minutes)
        if (TryParseMinutesVoor(input, out var voorTime))
            return voorTime;

        // "[uur] uur" ? hour:00
        if (TryParseExactHour(input, out var hourTime))
            return hourTime;

        return null;
    }

    private bool TryParseKwartOver(string input, out TimeSpan time)
    {
        var match = KwartOverRegex().Match(input);
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
        {
            time = new TimeSpan(hour, 15, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseKwartVoor(string input, out TimeSpan time)
    {
        var match = KwartVoorRegex().Match(input);
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
        {
            // Quarter to = 45 minutes of previous hour
            var actualHour = hour == 1 ? 12 : hour - 1;
            time = new TimeSpan(actualHour, 45, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseHalf(string input, out TimeSpan time)
    {
        // Dutch: "half vijf" = 4:30 (half to five, not half past four!)
        var match = HalfRegex().Match(input);
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
        {
            var actualHour = hour == 1 ? 12 : hour - 1;
            time = new TimeSpan(actualHour, 30, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseMinutesOver(string input, out TimeSpan time)
    {
        var match = MinutesOverRegex().Match(input);
        if (match.Success)
        {
            var minuteStr = match.Groups[1].Value;
            var hourStr = match.Groups[2].Value;

            if (TryParseMinuteWord(minuteStr, out var minutes) &&
                HourWords.TryGetValue(hourStr, out var hour))
            {
                time = new TimeSpan(hour, minutes, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private bool TryParseMinutesVoor(string input, out TimeSpan time)
    {
        var match = MinutesVoorRegex().Match(input);
        if (match.Success)
        {
            var minuteStr = match.Groups[1].Value;
            var hourStr = match.Groups[2].Value;

            if (TryParseMinuteWord(minuteStr, out var minutes) &&
                HourWords.TryGetValue(hourStr, out var hour))
            {
                var actualHour = hour == 1 ? 12 : hour - 1;
                time = new TimeSpan(actualHour, 60 - minutes, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private bool TryParseExactHour(string input, out TimeSpan time)
    {
        var match = ExactHourRegex().Match(input);
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
        // Handle both word form and numeric form
        if (int.TryParse(word, out minutes))
            return minutes >= 0 && minutes < 60;

        // "vijf" can mean 5 minutes when used with "over"/"voor"
        if (HourWords.TryGetValue(word, out minutes) && minutes <= 30)
            return true;

        minutes = 0;
        return false;
    }

    [GeneratedRegex(@"kwart over (\w+)")]
    private static partial Regex KwartOverRegex();

    [GeneratedRegex(@"kwart voor (\w+)")]
    private static partial Regex KwartVoorRegex();

    [GeneratedRegex(@"half (\w+)")]
    private static partial Regex HalfRegex();

    [GeneratedRegex(@"(\w+) over (\w+)")]
    private static partial Regex MinutesOverRegex();

    [GeneratedRegex(@"(\w+) voor (\w+)")]
    private static partial Regex MinutesVoorRegex();

    [GeneratedRegex(@"(\w+) uur")]
    private static partial Regex ExactHourRegex();
}
