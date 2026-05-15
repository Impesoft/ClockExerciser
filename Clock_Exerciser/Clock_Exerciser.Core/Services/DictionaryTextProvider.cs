using Clock_Exerciser.Core.Abstractions;

namespace Clock_Exerciser.Core.Services;

public sealed class DictionaryTextProvider : ITextProvider
{
    private readonly ICultureStore _cultureStore;

    private static readonly IReadOnlyDictionary<string, string> English = new Dictionary<string, string>
    {
        [AppTextKeys.AppTitle] = "Clock Trainer",
        [AppTextKeys.LanguageLabel] = "Language",
        [AppTextKeys.ModeClockToTime] = "Clock to time",
        [AppTextKeys.ModeTimeToClock] = "Time to clock",
        [AppTextKeys.ModeRandom] = "Random",
        [AppTextKeys.ClockToTimeInstruction] = "Read the clock and type the matching time (e.g. 15:30).",
        [AppTextKeys.TimeToClockInstruction] = "Move the hands to the requested time.",
        [AppTextKeys.EntryPlaceholder] = "Example: 03:45",
        [AppTextKeys.SubmitAnswer] = "Check",
        [AppTextKeys.NextChallenge] = "Next challenge",
        [AppTextKeys.ResultCorrect] = "Great job!",
        [AppTextKeys.ResultIncorrect] = "Close, try again.",
        [AppTextKeys.PromptLabel] = "Requested time",
        [AppTextKeys.HourLabel] = "Hour hand",
        [AppTextKeys.MinuteLabel] = "Minute hand",
        [AppTextKeys.SecondLabel] = "Second hand",
        [AppTextKeys.MenuTitle] = "Clock Exerciser",
        [AppTextKeys.MenuSubtitle] = "Learn to read the clock!",
        [AppTextKeys.BackToMenu] = "Menu",
        [AppTextKeys.QuitDialogTitle] = "Quit Clock Exerciser?",
        [AppTextKeys.QuitDialogMessage] = "Do you want to close the app?",
        [AppTextKeys.QuitDialogConfirm] = "Quit",
        [AppTextKeys.QuitDialogCancel] = "Cancel"
    };

    private static readonly IReadOnlyDictionary<string, string> Dutch = new Dictionary<string, string>
    {
        [AppTextKeys.AppTitle] = "Kloktrainer",
        [AppTextKeys.LanguageLabel] = "Taal",
        [AppTextKeys.ModeClockToTime] = "Klok naar tijd",
        [AppTextKeys.ModeTimeToClock] = "Tijd naar klok",
        [AppTextKeys.ModeRandom] = "Willekeurig",
        [AppTextKeys.ClockToTimeInstruction] = "Lees de klok en typ de juiste tijd (bijv. 15:30).",
        [AppTextKeys.TimeToClockInstruction] = "Zet de wijzers op de gevraagde tijd.",
        [AppTextKeys.EntryPlaceholder] = "Voorbeeld: 03:45",
        [AppTextKeys.SubmitAnswer] = "Controleer",
        [AppTextKeys.NextChallenge] = "Volgende opdracht",
        [AppTextKeys.ResultCorrect] = "Goed gedaan!",
        [AppTextKeys.ResultIncorrect] = "Bijna, probeer opnieuw.",
        [AppTextKeys.PromptLabel] = "Gevraagde tijd",
        [AppTextKeys.HourLabel] = "Uurwijzer",
        [AppTextKeys.MinuteLabel] = "Minutenwijzer",
        [AppTextKeys.SecondLabel] = "Secondewijzer",
        [AppTextKeys.MenuTitle] = "Klok Oefenen",
        [AppTextKeys.MenuSubtitle] = "Leer kloklezen!",
        [AppTextKeys.BackToMenu] = "Menu",
        [AppTextKeys.QuitDialogTitle] = "Klok Oefenen afsluiten?",
        [AppTextKeys.QuitDialogMessage] = "Wil je de app sluiten?",
        [AppTextKeys.QuitDialogConfirm] = "Afsluiten",
        [AppTextKeys.QuitDialogCancel] = "Annuleren"
    };

    public DictionaryTextProvider(ICultureStore cultureStore)
    {
        _cultureStore = cultureStore;
    }

    public string GetString(string key)
    {
        var dictionary = _cultureStore.CurrentCulture.TwoLetterISOLanguageName.Equals("nl", StringComparison.OrdinalIgnoreCase)
            ? Dutch
            : English;

        return dictionary.TryGetValue(key, out var value) ? value : key;
    }
}
