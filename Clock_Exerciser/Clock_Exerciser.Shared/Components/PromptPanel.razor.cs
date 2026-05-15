using Microsoft.AspNetCore.Components;

namespace Clock_Exerciser.Shared.Components;

public partial class PromptPanel
{
    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string PromptText { get; set; } = string.Empty;

    [Parameter]
    public string PromptDigital { get; set; } = string.Empty;
}
