using Microsoft.AspNetCore.Components;

namespace Clock_Exerciser.Shared.Components;

public partial class AnswerInput
{
    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    private async Task OnInput(ChangeEventArgs args)
    {
        await ValueChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);
    }
}
