using System.Globalization;

namespace Clock_Exerciser.Core.Models;

public sealed record LanguageOption(string DisplayName, string CultureName)
{
    public CultureInfo Culture => new(CultureName);
}
