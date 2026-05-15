using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Clock_Exerciser.Shared.Components;

public partial class AnalogClock
{
    private static readonly CultureInfo NumberFormat = CultureInfo.InvariantCulture;

    [Parameter]
    public double HourValue { get; set; }

    [Parameter]
    public double MinuteValue { get; set; }

    [Parameter]
    public double SecondValue { get; set; }

    [Parameter]
    public int Size { get; set; } = 320;

    [Parameter]
    public bool Compact { get; set; }

    private double HourAngle => NormalizeDialValue(HourValue) * 30d;

    private double MinuteAngle => NormalizeDialValue(MinuteValue) * 30d;

    private double SecondAngle => NormalizeDialValue(SecondValue) * 30d;

    private SvgPoint HourHandEnd => GetHandEndPoint(HourAngle, 48d);

    private SvgPoint MinuteHandEnd => GetHandEndPoint(MinuteAngle, 77d);

    private SvgPoint SecondHandEnd => GetHandEndPoint(SecondAngle, 88d);

    private static IReadOnlyList<double> MinorTicks { get; } = Enumerable.Range(0, 60)
        .Where(value => value % 5 != 0)
        .Select(value => value * 6d)
        .ToArray();

    private static IReadOnlyList<HourTick> HourTicks { get; } = Enumerable.Range(0, 12)
        .Select(value => new HourTick(value * 30d))
        .ToArray();

    private static IReadOnlyList<ClockLabel> Labels { get; } = Enumerable.Range(1, 12)
        .Select(value =>
        {
            var angle = (value % 12) * Math.PI / 6d - Math.PI / 2d;
            const double radius = 72d;
            return new ClockLabel(
                value.ToString(CultureInfo.InvariantCulture),
                (100d + radius * Math.Cos(angle)).ToString("0.###", NumberFormat),
                (100d + radius * Math.Sin(angle)).ToString("0.###", NumberFormat));
        })
        .ToArray();

    private static double NormalizeDialValue(double value)
    {
        var normalized = value % 12d;
        return normalized < 0 ? normalized + 12d : normalized;
    }

    private static MarkupString GetLabelMarkup(ClockLabel label)
    {
        return new MarkupString($"<text class=\"clock-label\" fill=\"currentColor\" x=\"{label.X}\" y=\"{label.Y}\" text-anchor=\"middle\" dominant-baseline=\"middle\">{label.Text}</text>");
    }

    private static SvgPoint GetHandEndPoint(double angleDegrees, double length)
    {
        var angleRadians = angleDegrees * Math.PI / 180d;
        var x = 100d + length * Math.Sin(angleRadians);
        var y = 100d - length * Math.Cos(angleRadians);

        return new SvgPoint(x.ToString("0.###", NumberFormat), y.ToString("0.###", NumberFormat));
    }

    private sealed record HourTick(double Angle);

    private sealed record ClockLabel(string Text, string X, string Y);

    private sealed record SvgPoint(string X, string Y);
}
