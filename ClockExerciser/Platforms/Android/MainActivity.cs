using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using System.Diagnostics;
using Debug = System.Diagnostics.Debug;

namespace ClockExerciser
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
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

        private void OnAndroidUnhandledException(object sender, RaiseThrowableEventArgs e)
        {
            Debug.WriteLine($"❌ ANDROID UNHANDLED EXCEPTION: {e.Exception.Message}");
            Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");
            Android.Util.Log.Error("ClockExerciser", $"Unhandled exception: {e.Exception}");
        }
    }
}
