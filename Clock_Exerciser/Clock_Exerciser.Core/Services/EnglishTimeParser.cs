using System.Text.RegularExpressions;

namespace Clock_Exerciser.Core.Services;

public sealed partial class EnglishTimeParser
{
    private static readonly Dictionary<string, int> HourWords = new()
    {
        ["one"] = 1,
        ["two"] = 2,
        ["three"] = 3,
        ["four"] = 4,
        ["five"] = 5,
        ["six"] = 6,
        ["seven"] = 7,
        ["eight"] = 8,
        ["nine"] = 9,
        ["ten"] = 10,
        ["eleven"] = 11,
        ["twelve"] = 12
    };

    private static readonly Dictionary<string, int> MinuteWords = new()
    {
        ["five"] = 5,
        ["ten"] = 10,
        ["quarter"] = 15,
        ["twenty"] = 20,
        ["twenty-five"] = 25,
        ["half"] = 30
    };

    public TimeSpan? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        input = input.Trim().ToLowerInvariant();

        if (TryParseQuarterPast(input, out var quarterPastTime))
        {
            return quarterPastTime;
        }

        if (TryParseQuarterTo(input, out var quarterToTime))
        {
            return quarterToTime;
        }

        if (TryParseHalfPast(input, out var halfPastTime))
        {
            return halfPastTime;
        }

        if (TryParseMinutesPast(input, out var pastTime))
        {
            return pastTime;
        }

        if (TryParseMinutesTo(input, out var toTime))
        {
            return toTime;
        }

        if (TryParseOClock(input, out var oclockTime))
        {
            return oclockTime;
        }

        return null;
    }

    private static bool TryParseQuarterPast(string input, out TimeSpan time)
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

    private static bool TryParseQuarterTo(string input, out TimeSpan time)
    {
        var match = QuarterToRegex().Match(input);
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
        {
            var actualHour = hour == 1 ? 12 : hour - 1;
            time = new TimeSpan(actualHour, 45, 0);
            return true;
        }

        time = default;
        return false;
    }

    private static bool TryParseHalfPast(string input, out TimeSpan time)
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

    private static bool TryParseMinutesPast(string input, out TimeSpan time)
    {
        var match = MinutesPastRegex().Match(input);
        if (match.Success)
        {
            var minuteText = match.Groups[1].Value;
            var hourText = match.Groups[2].Value;

            if (TryParseMinuteWord(minuteText, out var minutes) && HourWords.TryGetValue(hourText, out var hour))
            {
                time = new TimeSpan(hour, minutes, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private static bool TryParseMinutesTo(string input, out TimeSpan time)
    {
        var match = MinutesToRegex().Match(input);
        if (match.Success)
        {
            var minuteText = match.Groups[1].Value;
            var hourText = match.Groups[2].Value;

            if (TryParseMinuteWord(minuteText, out var minutes) && HourWords.TryGetValue(hourText, out var hour))
            {
                var actualHour = hour == 1 ? 12 : hour - 1;
                time = new TimeSpan(actualHour, 60 - minutes, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private static bool TryParseOClock(string input, out TimeSpan time)
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

    private static bool TryParseMinuteWord(string word, out int minutes)
    {
        if (MinuteWords.TryGetValue(word, out minutes))
        {
            return true;
        }

        if (int.TryParse(word, out minutes))
        {
            return minutes >= 0 && minutes < 60;
        }

        if (word.Contains('-'))
        {
            var parts = word.Split('-');
            if (parts.Length == 2)
            {
                var tens = parts[0] == "twenty" ? 20 : 0;
                var ones = HourWords.TryGetValue(parts[1], out var value) ? value : 0;
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

    [GeneratedRegex(@"quarter past (\w+)")]
    private static partial Regex QuarterPastRegex();

    [GeneratedRegex(@"quarter to (\w+)")]
    private static partial Regex QuarterToRegex();

    [GeneratedRegex(@"half past (\w+)")]
    private static partial Regex HalfPastRegex();

    [GeneratedRegex(@"([\w-]+) past (\w+)")]
    private static partial Regex MinutesPastRegex();

    [GeneratedRegex(@"([\w-]+) to (\w+)")]
    private static partial Regex MinutesToRegex();

    [GeneratedRegex(@"(\w+) o'?clock")]
    private static partial Regex OClockRegex();
}
