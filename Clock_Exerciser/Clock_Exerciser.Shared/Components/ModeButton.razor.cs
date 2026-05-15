using Microsoft.AspNetCore.Components;

namespace Clock_Exerciser.Shared.Components;

public partial class ModeButton
{
    [Parameter]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    public string Variant { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<Microsoft.AspNetCore.Components.Web.MouseEventArgs> OnClick { get; set; }
}
