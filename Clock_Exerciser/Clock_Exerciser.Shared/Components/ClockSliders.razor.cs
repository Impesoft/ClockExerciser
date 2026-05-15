using Microsoft.AspNetCore.Components;

namespace Clock_Exerciser.Shared.Components;

public partial class ClockSliders
{
    [Parameter]
    public string HourLabel { get; set; } = string.Empty;

    [Parameter]
    public string MinuteLabel { get; set; } = string.Empty;

    [Parameter]
    public double HourValue { get; set; }

    [Parameter]
    public EventCallback<double> HourValueChanged { get; set; }

    [Parameter]
    public double MinuteValue { get; set; }

    [Parameter]
    public EventCallback<double> MinuteValueChanged { get; set; }

    private async Task OnHourInput(ChangeEventArgs args)
    {
        if (double.TryParse(args.Value?.ToString(), out var value))
        {
            await HourValueChanged.InvokeAsync(value);
        }
    }

    private async Task OnMinuteInput(ChangeEventArgs args)
    {
        if (double.TryParse(args.Value?.ToString(), out var value))
        {
            await MinuteValueChanged.InvokeAsync(value);
        }
    }
}
