using System.Globalization;

namespace Clock_Exerciser.Core.Services;

public static class TimePhraseFormatter
{
    public static string FormatFriendlyTime(TimeSpan time, CultureInfo culture)
    {
        var hour = time.Hours % 12;
        if (hour == 0)
        {
            hour = 12;
        }

        var nextHour = (hour % 12) + 1;
        var minute = time.Minutes;

        return culture.TwoLetterISOLanguageName.Equals("nl", StringComparison.OrdinalIgnoreCase)
            ? CreateDutchPhrase(hour, nextHour, minute)
            : CreateEnglishPhrase(hour, nextHour, minute);
    }

    private static string CreateDutchPhrase(int hour, int nextHour, int minute)
    {
        string HourWord(int value) => value switch
        {
            1 => "een",
            2 => "twee",
            3 => "drie",
            4 => "vier",
            5 => "vijf",
            6 => "zes",
            7 => "zeven",
            8 => "acht",
            9 => "negen",
            10 => "tien",
            11 => "elf",
            12 => "twaalf",
            _ => value.ToString(CultureInfo.InvariantCulture)
        };

        return minute switch
        {
            0 => $"{HourWord(hour)} uur",
            15 => $"kwart over {HourWord(hour)}",
            30 => $"half {HourWord(nextHour)}",
            45 => $"kwart voor {HourWord(nextHour)}",
            _ when minute < 30 => $"{minute} over {HourWord(hour)}",
            _ => $"{60 - minute} voor {HourWord(nextHour)}"
        };
    }

    private static string CreateEnglishPhrase(int hour, int nextHour, int minute)
    {
        string HourWord(int value) => value switch
        {
            1 => "one",
            2 => "two",
            3 => "three",
            4 => "four",
            5 => "five",
            6 => "six",
            7 => "seven",
            8 => "eight",
            9 => "nine",
            10 => "ten",
            11 => "eleven",
            12 => "twelve",
            _ => value.ToString(CultureInfo.InvariantCulture)
        };

        return minute switch
        {
            0 => $"{HourWord(hour)} o'clock",
            15 => $"quarter past {HourWord(hour)}",
            30 => $"half past {HourWord(hour)}",
            45 => $"quarter to {HourWord(nextHour)}",
            _ when minute < 30 => $"{minute} past {HourWord(hour)}",
            _ => $"{60 - minute} to {HourWord(nextHour)}"
        };
    }
}
