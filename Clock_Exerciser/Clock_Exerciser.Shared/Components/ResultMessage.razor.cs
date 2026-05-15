using Microsoft.AspNetCore.Components;

namespace Clock_Exerciser.Shared.Components;

public partial class ResultMessage
{
    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public bool Success { get; set; }

    [Parameter]
    public string Message { get; set; } = string.Empty;
}
