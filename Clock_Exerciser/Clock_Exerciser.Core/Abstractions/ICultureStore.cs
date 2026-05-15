using System.Globalization;

namespace Clock_Exerciser.Core.Abstractions;

public interface ICultureStore
{
    CultureInfo CurrentCulture { get; }

    event EventHandler<CultureInfo>? CultureChanged;

    ValueTask SetCultureAsync(CultureInfo culture, CancellationToken cancellationToken = default);
}
