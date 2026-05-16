using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Clock_Exerciser.Shared.Components;

public partial class AnswerInput : IAsyncDisposable
{
    private ElementReference inputElement;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public EventCallback OnEnterKeyPressed { get; set; }

    private async Task OnInput(ChangeEventArgs args)
    {
        await ValueChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);
    }

    private async Task OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" && OnEnterKeyPressed.HasDelegate)
        {
            // Simple blur - let the button focus handle keyboard dismissal
            await BlurInputAsync();
            await OnEnterKeyPressed.InvokeAsync();
        }
    }

    private async Task BlurInputAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", "arguments[0].blur()", inputElement);
        }
        catch
        {
            // Silently fail
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Nothing to dispose anymore
        await Task.CompletedTask;
    }
}
