using Android.Content;
using Android.Speech;
using AndroidX.Core.Content;
using ClockExerciser.Services;
using System.Globalization;

namespace ClockExerciser.Platforms.Android;

public class SpeechRecognitionService : ISpeechRecognitionService
{
    private TaskCompletionSource<string?>? _tcs;

    public SpeechRecognitionService()
    {
        // Register this service with MainActivity
        MainActivity.SetSpeechRecognitionService(this);
    }

    public async Task<string?> RecognizeAsync(string locale = "en-US")
    {
        try
        {
            // Check permission first
            if (!await RequestPermissionsAsync())
            {
                System.Diagnostics.Debug.WriteLine("?? Microphone permission denied");
                return null;
            }

            // Check if recognition is available
            if (!await IsAvailableAsync())
            {
                System.Diagnostics.Debug.WriteLine("?? Speech recognition not available");
                return null;
            }

            _tcs = new TaskCompletionSource<string?>();

            var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            
            // Set locale
            var culture = new CultureInfo(locale);
            intent.PutExtra(RecognizerIntent.ExtraLanguage, culture.Name);
            
            intent.PutExtra(RecognizerIntent.ExtraPrompt, "Speak the time...");
            intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

            // Start the intent on the UI thread
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    var activity = Platform.CurrentActivity;
                    if (activity != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"?? Starting speech recognition with locale: {culture.Name}");
                        activity.StartActivityForResult(intent, 1001);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("?? ERROR: No current activity");
                        _tcs?.TrySetResult(null);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"?? Error starting speech recognition: {ex.Message}");
                    _tcs?.TrySetException(ex);
                }
            });

            // Wait for result with timeout
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var completedTask = await Task.WhenAny(_tcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                System.Diagnostics.Debug.WriteLine("?? Speech recognition timed out");
                _tcs?.TrySetResult(null);
                return null;
            }

            return await _tcs.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"?? Speech recognition error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> RequestPermissionsAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.Android)
            {
                // User previously denied permission
                System.Diagnostics.Debug.WriteLine("?? Microphone permission was previously denied");
                return false;
            }

            status = await Permissions.RequestAsync<Permissions.Microphone>();
            System.Diagnostics.Debug.WriteLine($"?? Microphone permission status: {status}");
            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"?? Permission request error: {ex.Message}");
            return false;
        }
    }

    public Task<bool> IsAvailableAsync()
    {
        try
        {
            var activities = SpeechRecognizer.IsRecognitionAvailable(global::Android.App.Application.Context);
            return Task.FromResult(activities);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public void SetRecognitionResult(string? result)
    {
        _tcs?.TrySetResult(result);
    }

    public void CancelRecognition()
    {
        _tcs?.TrySetResult(null);
    }
}
