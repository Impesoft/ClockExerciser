using Clock_Exerciser.Core.Abstractions;
using Clock_Exerciser.Core.Services;
using Clock_Exerciser.Web.Components;
using Clock_Exerciser.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<IPreferenceStore, WebPreferenceStore>();
builder.Services.AddScoped<ICultureStore, AppCultureStore>();
builder.Services.AddScoped<ITextProvider, DictionaryTextProvider>();
builder.Services.AddScoped<IAudioFeedbackService, WebAudioFeedbackService>();
builder.Services.AddTransient<IClockTicker, SystemClockTicker>();
builder.Services.AddScoped<ChallengeGenerator>();
builder.Services.AddScoped<DutchTimeParser>();
builder.Services.AddScoped<EnglishTimeParser>();
builder.Services.AddScoped<UserTimeParser>();
builder.Services.AddScoped<AnswerValidator>();
builder.Services.AddScoped<ClockExerciseState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(Clock_Exerciser.Shared._Imports).Assembly);

app.Run();
