using Clock_Exerciser.Core.Models;
using Microsoft.AspNetCore.Components;

namespace Clock_Exerciser.Shared.Components;

public partial class LanguageSelector
{
    [Parameter]
    public string Id { get; set; } = $"language-{Guid.NewGuid():N}";

    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string AriaLabel { get; set; } = "Language";

    [Parameter]
    public string CssClass { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<LanguageOption> Languages { get; set; } = [];

    [Parameter]
    public string SelectedCultureName { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> SelectedCultureNameChanged { get; set; }

    private async Task OnChangedAsync(ChangeEventArgs args)
    {
        await SelectedCultureNameChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);
    }
}
