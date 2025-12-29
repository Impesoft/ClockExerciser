using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using ClockExerciser.Platforms.Android;
using System.Diagnostics;
using Debug = System.Diagnostics.Debug;

namespace ClockExerciser
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private static SpeechRecognitionService? _speechRecognitionService;

        public static void SetSpeechRecognitionService(SpeechRecognitionService service)
        {
            _speechRecognitionService = service;
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            try
            {
                Debug.WriteLine("📱 MainActivity.OnCreate starting...");
                
                // Add Android-specific crash handler
                AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidUnhandledException;
                
                base.OnCreate(savedInstanceState);
                
                Debug.WriteLine("✅ MainActivity.OnCreate completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ FATAL: MainActivity.OnCreate failed: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                Android.Util.Log.Error("ClockExerciser", $"MainActivity crash: {ex}");
                throw;
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Handle speech recognition result (requestCode 1001)
            if (requestCode == 1001)
            {
                if (resultCode == Result.Ok && data != null)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches != null && matches.Count > 0)
                    {
                        var recognizedText = matches[0];
                        Debug.WriteLine($"🎤 Speech recognized: {recognizedText}");
                        _speechRecognitionService?.SetRecognitionResult(recognizedText);
                    }
                    else
                    {
                        Debug.WriteLine("🎤 No speech matches found");
                        _speechRecognitionService?.SetRecognitionResult(null);
                    }
                }
                else
                {
                    Debug.WriteLine($"🎤 Speech recognition cancelled or failed: {resultCode}");
                    _speechRecognitionService?.CancelRecognition();
                }
            }
        }

        private void OnAndroidUnhandledException(object sender, RaiseThrowableEventArgs e)
        {
            Debug.WriteLine($"❌ ANDROID UNHANDLED EXCEPTION: {e.Exception.Message}");
            Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");
            Android.Util.Log.Error("ClockExerciser", $"Unhandled exception: {e.Exception}");
        }
    }
}
