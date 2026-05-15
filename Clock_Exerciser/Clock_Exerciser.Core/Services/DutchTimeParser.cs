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

        input = input.Trim().ToLowerInvariant();

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

    private static bool TryParseKwartOver(string input, out TimeSpan time)
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

    private static bool TryParseKwartVoor(string input, out TimeSpan time)
    {
        var match = KwartVoorRegex().Match(input);
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
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
        if (match.Success && HourWords.TryGetValue(match.Groups[1].Value, out var hour))
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

            if (TryParseMinuteWord(minuteText, out var minutes) && HourWords.TryGetValue(hourText, out var hour))
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

    private static bool TryParseExactHour(string input, out TimeSpan time)
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

    private static bool TryParseMinuteWord(string word, out int minutes)
    {
        if (int.TryParse(word, out minutes))
        {
            return minutes >= 0 && minutes < 60;
        }

        if (HourWords.TryGetValue(word, out minutes) && minutes <= 30)
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

    [GeneratedRegex(@"(\w+) over (\w+)")]
    private static partial Regex MinutesOverRegex();

    [GeneratedRegex(@"(\w+) voor (\w+)")]
    private static partial Regex MinutesVoorRegex();

    [GeneratedRegex(@"(\w+) uur")]
    private static partial Regex ExactHourRegex();
}
