using ClockExerciser.Helpers;
using ClockExerciser.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using Syncfusion.Licensing;
using Syncfusion.Maui.Core.Hosting;
using System.Diagnostics;

namespace ClockExerciser
{
    public static class MauiProgram
    {
        private static void Log(string message)
        {
            Debug.WriteLine(message);
#if ANDROID
            Android.Util.Log.Info("ClockExerciser_Startup", message);
#endif
        }

        public static MauiApp CreateMauiApp()
        {
            try
            {
                Log("🚀 Starting MauiApp creation...");

                var builder = MauiApp.CreateBuilder();

                builder.Configuration
                    .AddJsonFile("secrets.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables(prefix: "CLOCKEXERCISER_");

                Log("📝 Registering Syncfusion license...");
                try
                {
                    var licenseKey = builder.Configuration["Syncfusion:LicenseKey"]
                                     ?? builder.Configuration["ClockExerciser:Syncfusion:LicenseKey"]
                                     ?? Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");

                    if (string.IsNullOrWhiteSpace(licenseKey))
                    {
                        Log("⚠️ WARNING: Syncfusion license key not configured. Add it to `ClockExerciser/secrets.json` (Syncfusion:LicenseKey) or set env var `SYNCFUSION_LICENSE_KEY`.");
                    }
                    else
                    {
                        SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                        Log("✅ Syncfusion license registered successfully");
                    }
                }
                catch (Exception ex)
                {
                    Log($"⚠️ WARNING: Syncfusion license registration failed: {ex.Message}");
                    Log($"Exception type: {ex.GetType().Name}");
                    Log($"This might be a network issue or license validation problem");
                }

                Log("⚙️ Configuring MAUI app...");
                builder
                    .UseMauiApp<App>()
                    .ConfigureSyncfusionCore()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

                Log("🔊 Registering audio manager...");
                // Register audio manager
                var audioManager = AudioManager.Current;
                if (audioManager == null)
                {
                    Log("⚠️ WARNING: AudioManager.Current is null. Audio features will not work.");
                }
                else
                {
                    builder.Services.AddSingleton(audioManager);
                    Log("✅ Audio manager registered");
                }

                Log("📦 Registering services...");
                builder.Services.AddSingleton<LocalizationService>();
                builder.Services.AddSingleton<IAudioService, AudioService>();
                builder.Services.AddSingleton<DutchTimeParser>();
                builder.Services.AddSingleton<EnglishTimeParser>();
                builder.Services.AddTransient<ViewModels.GameViewModel>();
                builder.Services.AddTransient<ViewModels.MenuViewModel>();
                builder.Services.AddTransient<GamePage>();
                builder.Services.AddTransient<MenuPage>();
                Log("✅ All services registered");

#if DEBUG
                builder.Logging.AddDebug();
                Log("🐛 Debug logging enabled");
#endif

                Log("🏗️ Building app...");
                var app = builder.Build();

                Log("🔗 Initializing ServiceHelper...");
                ServiceHelper.Initialize(app.Services);

                Log("✅ MauiApp created successfully!");
                return app;
            }
            catch (Exception ex)
            {
                var msg = $"❌ FATAL ERROR in CreateMauiApp: {ex.Message} | Type: {ex.GetType().Name} | Stack: {ex.StackTrace}";
                Log(msg);
#if ANDROID
                Android.Util.Log.Error("ClockExerciser_Startup", msg);
                if (ex.InnerException != null)
                {
                    Android.Util.Log.Error("ClockExerciser_Startup", $"Inner: {ex.InnerException.Message}");
                }
#endif
                throw;
            }
        }
    }
}
