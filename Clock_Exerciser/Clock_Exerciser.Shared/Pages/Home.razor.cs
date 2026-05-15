using Clock_Exerciser.Core.Models;
using Clock_Exerciser.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Clock_Exerciser.Shared.Pages;

public partial class Home : IAsyncDisposable
{
    private IJSObjectReference? _deviceMetricsModule;
    private DotNetObjectReference<Home>? _dotNetReference;
    private string? _deviceMetricsListenerId;
    private bool showResolution = false;

    [Inject]
    private ClockExerciseState State { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private string CurrentLanguageName => State.GetCurrentLanguage().CultureName;

    private string? DeviceMetricsText { get; set; }

    protected override async Task OnInitializedAsync()
    {
        State.StateChanged += OnStateChanged;
        await State.InitializeAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _deviceMetricsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Clock_Exerciser.Shared/deviceMetrics.js");
        var metrics = await _deviceMetricsModule.InvokeAsync<DeviceMetricsSnapshot>("getDeviceMetrics");
        DeviceMetricsText = FormatDeviceMetrics(metrics);

        _dotNetReference = DotNetObjectReference.Create(this);
        _deviceMetricsListenerId = await _deviceMetricsModule.InvokeAsync<string>("addDeviceMetricsListener", _dotNetReference);
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        State.StateChanged -= OnStateChanged;

        if (_deviceMetricsModule is not null && _deviceMetricsListenerId is not null)
        {
            try
            {
                await _deviceMetricsModule.InvokeVoidAsync("removeDeviceMetricsListener", _deviceMetricsListenerId);
            }
            catch (JSDisconnectedException)
            {
            }
        }

        _dotNetReference?.Dispose();

        if (_deviceMetricsModule is not null)
        {
            try
            {
                await _deviceMetricsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }

    [JSInvokable]
    public Task OnDeviceMetricsChanged(DeviceMetricsSnapshot metrics)
    {
        DeviceMetricsText = FormatDeviceMetrics(metrics);
        return InvokeAsync(StateHasChanged);
    }

    private void NavigateClockToTime() => NavigateToGame(GameMode.ClockToTime);

    private void NavigateTimeToClock() => NavigateToGame(GameMode.TimeToClock);

    private void NavigateRandom() => NavigateToGame(GameMode.Random);

    private void NavigateToGame(GameMode mode)
    {
        NavigationManager.NavigateTo($"/game/{mode}");
    }

    private async Task OnLanguageChangedAsync(string cultureName)
    {
        var language = State.Languages.FirstOrDefault(option => option.CultureName == cultureName);
        if (language is not null)
        {
            await State.SetCultureAsync(language);
        }
    }

    private void OnStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private static string FormatDeviceMetrics(DeviceMetricsSnapshot metrics)
    {
        return $"Viewport {metrics.ViewportWidth:0}×{metrics.ViewportHeight:0} css | Screen {metrics.ScreenWidth}×{metrics.ScreenHeight} css | DPR {metrics.DevicePixelRatio:0.##} | Est. px {metrics.PhysicalWidth}×{metrics.PhysicalHeight} | {metrics.Orientation}";
    }

    public sealed class DeviceMetricsSnapshot
    {
        public string UserAgent { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public int AvailableWidth { get; set; }
        public int AvailableHeight { get; set; }
        public double ViewportWidth { get; set; }
        public double ViewportHeight { get; set; }
        public int InnerWidth { get; set; }
        public int InnerHeight { get; set; }
        public int OuterWidth { get; set; }
        public int OuterHeight { get; set; }
        public double DevicePixelRatio { get; set; }
        public int PhysicalWidth { get; set; }
        public int PhysicalHeight { get; set; }
        public string Orientation { get; set; } = string.Empty;
        public int ColorDepth { get; set; }
    }
}
