using Clock_Exerciser.Web.Client;
using Clock_Exerciser.Core.Abstractions;
using Clock_Exerciser.Core.Services;
using Clock_Exerciser.Web.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BrowserPreferenceStore>();
builder.Services.AddScoped<IPreferenceStore>(sp => sp.GetRequiredService<BrowserPreferenceStore>());
builder.Services.AddScoped<AppCultureStore>();
builder.Services.AddScoped<ICultureStore>(sp => sp.GetRequiredService<AppCultureStore>());
builder.Services.AddScoped<ITextProvider, DictionaryTextProvider>();
builder.Services.AddScoped<IAudioFeedbackService, BrowserAudioFeedbackService>();
builder.Services.AddTransient<IClockTicker, SystemClockTicker>();
builder.Services.AddScoped<ChallengeGenerator>();
builder.Services.AddScoped<DutchTimeParser>();
builder.Services.AddScoped<EnglishTimeParser>();
builder.Services.AddScoped<UserTimeParser>();
builder.Services.AddScoped<AnswerValidator>();
builder.Services.AddScoped<ClockExerciseState>();

var host = builder.Build();
await host.Services.GetRequiredService<AppCultureStore>().InitializeAsync();
await host.RunAsync();
