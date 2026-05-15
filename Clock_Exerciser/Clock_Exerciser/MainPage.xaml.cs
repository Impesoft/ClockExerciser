using Clock_Exerciser.Core.Abstractions;
using Clock_Exerciser.Core.Services;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Clock_Exerciser;

public partial class MainPage : ContentPage
{
    private readonly ICultureStore _cultureStore;
    private readonly ITextProvider _textProvider;

    public MainPage(ICultureStore cultureStore, ITextProvider textProvider)
    {
        InitializeComponent();
        _cultureStore = cultureStore;
        _textProvider = textProvider;
    }

    protected override bool OnBackButtonPressed()
    {
        // Handle back button asynchronously without blocking
        HandleBackButtonAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.WriteLine($"Back button handling failed: {task.Exception}");
            }
        });

        // Always return true to indicate we've handled the event
        return true;
    }

    private async Task HandleBackButtonAsync()
    {
        try
        {
            // Track whether we're on the menu page
            bool isOnMenu = false;

            // Use BlazorWebView to dispatch into the Blazor context
            await blazorWebView.TryDispatchAsync(sp =>
            {
                var navManager = sp.GetRequiredService<NavigationManager>();

                // Get current route relative to base URI
                var currentUri = new Uri(navManager.Uri);
                var baseUri = new Uri(navManager.BaseUri);
                var relativePath = baseUri.MakeRelativeUri(currentUri).ToString();

                // Check if we're on the menu (home page)
                isOnMenu = string.IsNullOrEmpty(relativePath) || relativePath == "/";
            });

            if (isOnMenu)
            {
                // We're on the menu, show quit confirmation
                var title = _textProvider.GetString(AppTextKeys.QuitDialogTitle);
                var message = _textProvider.GetString(AppTextKeys.QuitDialogMessage);
                var confirm = _textProvider.GetString(AppTextKeys.QuitDialogConfirm);
                var cancel = _textProvider.GetString(AppTextKeys.QuitDialogCancel);

                var result = await DisplayAlertAsync(title, message, confirm, cancel);

                if (result)
                {
                    // User confirmed quit
                    Application.Current?.Quit();
                }
            }
            else
            {
                // We're not on the menu, navigate back to menu
                await blazorWebView.TryDispatchAsync(sp =>
                {
                    var navMgr = sp.GetRequiredService<NavigationManager>();
                    navMgr.NavigateTo("/", forceLoad: false);
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling back button: {ex}");
        }
    }
}
