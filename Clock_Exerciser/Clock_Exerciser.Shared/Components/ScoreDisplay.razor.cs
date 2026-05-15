using Microsoft.AspNetCore.Components;

namespace Clock_Exerciser.Shared.Components;

public partial class ScoreDisplay
{
    [Parameter]
    public string Label { get; set; } = "Score:";

    [Parameter]
    public string ScoreText { get; set; } = string.Empty;
}
