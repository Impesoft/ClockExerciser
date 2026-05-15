using Clock_Exerciser.Core.Abstractions;
using Clock_Exerciser.Core.Services;
using Clock_Exerciser.Services;
using Microsoft.Extensions.Logging;

namespace Clock_Exerciser;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<IPreferenceStore, MauiPreferenceStore>();
        builder.Services.AddSingleton<ICultureStore, AppCultureStore>();
        builder.Services.AddSingleton<ITextProvider, DictionaryTextProvider>();
        builder.Services.AddSingleton<IAudioFeedbackService, MauiAudioFeedbackService>();
        builder.Services.AddTransient<IClockTicker, SystemClockTicker>();
        builder.Services.AddSingleton<ChallengeGenerator>();
        builder.Services.AddSingleton<DutchTimeParser>();
        builder.Services.AddSingleton<EnglishTimeParser>();
        builder.Services.AddSingleton<UserTimeParser>();
        builder.Services.AddSingleton<AnswerValidator>();
        builder.Services.AddScoped<ClockExerciseState>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
