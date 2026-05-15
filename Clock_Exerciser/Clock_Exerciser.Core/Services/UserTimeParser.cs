using System.Globalization;

namespace Clock_Exerciser.Core.Services;

public sealed class UserTimeParser
{
    private readonly DutchTimeParser _dutchTimeParser;
    private readonly EnglishTimeParser _englishTimeParser;

    public UserTimeParser(DutchTimeParser dutchTimeParser, EnglishTimeParser englishTimeParser)
    {
        _dutchTimeParser = dutchTimeParser;
        _englishTimeParser = englishTimeParser;
    }

    public bool TryParse(string? input, CultureInfo culture, out TimeSpan time)
    {
        input = input?.Trim();
        if (string.IsNullOrEmpty(input))
        {
            time = default;
            return false;
        }

        var formats = new[] { "h\\:mm", "hh\\:mm", "H\\:mm", "HH\\:mm" };
        foreach (var format in formats)
        {
            if (TimeSpan.TryParseExact(input, format, CultureInfo.InvariantCulture, out time))
            {
                return true;
            }
        }

        var parsedTime = culture.TwoLetterISOLanguageName.Equals("nl", StringComparison.OrdinalIgnoreCase)
            ? _dutchTimeParser.Parse(input)
            : _englishTimeParser.Parse(input);

        if (parsedTime.HasValue)
        {
            time = parsedTime.Value;
            return true;
        }

        time = default;
        return false;
    }
}
