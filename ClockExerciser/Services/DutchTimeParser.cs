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

    private static readonly Dictionary<string, int> MinuteWords = new()
    {
        {"een", 1}, {"twee", 2}, {"drie", 3}, {"vier", 4},
        {"vijf", 5}, {"zes", 6}, {"zeven", 7}, {"acht", 8},
        {"negen", 9}, {"tien", 10}, {"elf", 11}, {"twaalf", 12},
        {"dertien", 13}, {"veertien", 14}, {"vijftien", 15},
        {"zestien", 16}, {"zeventien", 17}, {"achttien", 18},
        {"negentien", 19}, {"twintig", 20}, {"eenentwintig", 21},
        {"tweeëntwintig", 22}, {"drieëntwintig", 23}, {"vierentwintig", 24},
        {"vijfentwintig", 25}, {"zesentwintig", 26}, {"zevenentwintig", 27},
        {"achtentwintig", 28}, {"negenentwintig", 29}, {"dertig", 30}
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

        // Check compound patterns FIRST (more specific)
        // "[minuten] voor half [uur]" ? (hour-1):(30-minutes) - e.g., "5 voor half twaalf" = 11:25
        if (TryParseMinutenVoorHalf(input, out var voorHalfTime))
            return voorHalfTime;

        // "[minuten] over/na half [uur]" ? (hour-1):(30+minutes) - e.g., "5 na half drie" = 2:35
        if (TryParseMinutenOverHalf(input, out var overHalfTime))
            return overHalfTime;

        // Then check simple patterns
        // "kwart over [uur]" ? :15
        if (TryParseKwartOver(input, out var kwartOverTime))
            return kwartOverTime;

        // "kwart voor [uur]" ? :45 (15 minutes before next hour)
        if (TryParseKwartVoor(input, out var kwartVoorTime))
            return kwartVoorTime;

        // "half [uur]" ? :30 (30 minutes before the hour)
        if (TryParseHalf(input, out var halfTime))
            return halfTime;

        // "[minuten] over/na [uur]" ? hour:minutes
        if (TryParseMinutenOver(input, out var overTime))
            return overTime;

        // "[minuten] voor [uur]" ? (hour-1):(60-minutes)
        if (TryParseMinutenVoor(input, out var voorTime))
            return voorTime;

        // "[uur] uur" ? hour:00
        if (TryParseExactUur(input, out var uurTime))
            return uurTime;

        return null;
    }

    private bool TryParseKwartOver(string input, out TimeSpan time)
    {
        var match = KwartOverRegex().Match(input);
        if (match.Success && TryParseUurWoord(match.Groups[1].Value, out var hour))
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
        if (match.Success && TryParseUurWoord(match.Groups[1].Value, out var hour))
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
        if (match.Success && TryParseUurWoord(match.Groups[1].Value, out var hour))
        {
            var actualHour = hour == 1 ? 12 : hour - 1;
            time = new TimeSpan(actualHour, 30, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseMinutenVoorHalf(string input, out TimeSpan time)
    {
        // "5 voor half twaalf" = 11:25 (5 minutes before 11:30)
        var match = MinutenVoorHalfRegex().Match(input);
        if (match.Success)
        {
            var minutenStr = match.Groups[1].Value;
            var uurStr = match.Groups[2].Value;

            if (TryParseMinuutWoord(minutenStr, out var minuten) &&
                TryParseUurWoord(uurStr, out var uur))
            {
                var actueleUur = uur == 1 ? 12 : uur - 1;
                var actueleMinuten = 30 - minuten;
                if (actueleMinuten < 0)
                {
                    actueleMinuten += 60;
                    actueleUur = actueleUur == 1 ? 12 : actueleUur - 1;
                }
                time = new TimeSpan(actueleUur, actueleMinuten, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private bool TryParseMinutenOverHalf(string input, out TimeSpan time)
    {
        // "5 over half twaalf" = 11:35 (5 minutes after 11:30)
        var match = MinutenOverHalfRegex().Match(input);
        if (match.Success)
        {
            var minutenStr = match.Groups[1].Value;
            var uurStr = match.Groups[2].Value;

            if (TryParseMinuutWoord(minutenStr, out var minuten) &&
                TryParseUurWoord(uurStr, out var uur))
            {
                var actueleUur = uur == 1 ? 12 : uur - 1;
                var actueleMinuten = 30 + minuten;
                if (actueleMinuten >= 60)
                {
                    actueleMinuten -= 60;
                    actueleUur = actueleUur % 12 + 1;
                }
                time = new TimeSpan(actueleUur, actueleMinuten, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private bool TryParseMinutenOver(string input, out TimeSpan time)
    {
        var match = MinutenOverRegex().Match(input);
        if (match.Success)
        {
            var minutenStr = match.Groups[1].Value;
            var uurStr = match.Groups[2].Value;

            if (TryParseMinuutWoord(minutenStr, out var minuten) &&
                TryParseUurWoord(uurStr, out var uur))
            {
                time = new TimeSpan(uur, minuten, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private bool TryParseMinutenVoor(string input, out TimeSpan time)
    {
        var match = MinutenVoorRegex().Match(input);
        if (match.Success)
        {
            var minutenStr = match.Groups[1].Value;
            var uurStr = match.Groups[2].Value;

            if (TryParseMinuutWoord(minutenStr, out var minuten) &&
                TryParseUurWoord(uurStr, out var uur))
            {
                var actueleUur = uur == 1 ? 12 : uur - 1;
                time = new TimeSpan(actueleUur, 60 - minuten, 0);
                return true;
            }
        }

        time = default;
        return false;
    }

    private bool TryParseExactUur(string input, out TimeSpan time)
    {
        var match = ExactUurRegex().Match(input);
        if (match.Success && TryParseUurWoord(match.Groups[1].Value, out var uur))
        {
            time = new TimeSpan(uur, 0, 0);
            return true;
        }

        time = default;
        return false;
    }

    private bool TryParseMinuutWoord(string woord, out int minuten)
    {
        // Try numeric form first
        if (int.TryParse(woord, out minuten))
            return minuten >= 0 && minuten < 60;

        // Try minute words dictionary
        if (MinuteWords.TryGetValue(woord, out minuten))
            return true;

        minuten = 0;
        return false;
    }

    private bool TryParseUurWoord(string woord, out int uur)
    {
        // Try hour words dictionary
        if (HourWords.TryGetValue(woord, out uur))
            return true;

        // Try numeric form (1-12)
        if (int.TryParse(woord, out uur))
            return uur >= 1 && uur <= 12;

        uur = 0;
        return false;
    }

    [GeneratedRegex(@"kwart over (\w+)")]
    private static partial Regex KwartOverRegex();

    [GeneratedRegex(@"kwart voor (\w+)")]
    private static partial Regex KwartVoorRegex();

    [GeneratedRegex(@"half (\w+)")]
    private static partial Regex HalfRegex();

    [GeneratedRegex(@"(\w+) voor half (\w+)")]
    private static partial Regex MinutenVoorHalfRegex();

    [GeneratedRegex(@"(\w+) (?:over|na) half (\w+)")]
    private static partial Regex MinutenOverHalfRegex();

    [GeneratedRegex(@"(\w+) (?:over|na) (\w+)")]
    private static partial Regex MinutenOverRegex();

    [GeneratedRegex(@"(\w+) voor (\w+)")]
    private static partial Regex MinutenVoorRegex();

    [GeneratedRegex(@"(\w+) uur")]
    private static partial Regex ExactUurRegex();
}
