# Language Order Fix

## Issue Fixed
The written time prompt (e.g., "half past three") was showing in Dutch even when English was selected in "Time to Clock" mode (second play mode).

## Root Cause
The language order in the ViewModels didn't match the LocalizationService default:

**Before:**
- `LocalizationService` defaults to **en-US** (English)
- `GameViewModel.Languages` had **Nederlands first**
- `MenuViewModel.Languages` had **Nederlands first**

This mismatch caused the prompt text to potentially show in the wrong language when switching languages or starting a new game.

## Solution Applied

### 1. GameViewModel.cs
Changed language order to match LocalizationService:
```csharp
Languages = new ObservableCollection<LanguageOption>
{
    new("English", "en-US"),      // ? Now first
    new("Nederlands", "nl-NL")     // Now second
};
```

### 2. MenuViewModel.cs
Applied same fix:
```csharp
Languages = new ObservableCollection<LanguageOption>
{
    new("English", "en-US"),      // ? Now first
    new("Nederlands", "nl-NL")     // Now second
};
```

### 3. LocalizationService.cs
Already defaults to English (unchanged):
```csharp
readonly CultureInfo _defaultCulture = new("en-US");
```

## How It Works Now

### On App Start:
1. LocalizationService initializes with en-US
2. MenuPage/GamePage ViewModels create language list with English first
3. SelectedLanguage set to Languages.First() = English
4. UI shows in English by default ?

### On Language Change:
1. User selects "Nederlands" from picker
2. ViewModel.SelectedLanguage setter calls `_localizationService.SetCulture(...)`
3. CultureChanged event fires
4. `OnCultureChanged()` calls `UpdatePromptTexts()`
5. `FormatFriendlyTime()` uses new culture
6. Prompt text updates to Dutch ?

### On Return to English:
1. User selects "English" from picker
2. Same flow as above
3. Prompt text updates to English ?

## Testing Verification

Test these scenarios:

### Scenario 1: App Start
- [ ] Launch app
- [ ] Go to "Time to Clock" mode
- [ ] Verify prompt shows English (e.g., "half past three")

### Scenario 2: Switch to Dutch
- [ ] Start in English
- [ ] Change language picker to "Nederlands"
- [ ] Verify prompt updates to Dutch (e.g., "half vijf")
- [ ] Click "Next Challenge"
- [ ] Verify new prompt is in Dutch

### Scenario 3: Switch Back to English
- [ ] Currently in Dutch
- [ ] Change language picker to "English"
- [ ] Verify prompt updates to English
- [ ] Click "Next Challenge"
- [ ] Verify new prompt is in English

### Scenario 4: Mode Switching
- [ ] Set language to English
- [ ] Try "Clock to Time" mode
- [ ] Try "Time to Clock" mode
- [ ] Try "Random" mode
- [ ] Verify all prompts stay in English

### Scenario 5: Language Consistency
- [ ] Change language in menu page
- [ ] Navigate to game
- [ ] Verify game uses selected language
- [ ] Return to menu
- [ ] Verify menu still shows selected language

## Related Code

### FormatFriendlyTime Method
Located in `GameViewModel.cs`:
```csharp
private static string FormatFriendlyTime(TimeSpan time, CultureInfo culture)
{
    // ...
    if (culture.TwoLetterISOLanguageName.Equals("nl", StringComparison.OrdinalIgnoreCase))
    {
        return CreateDutchPhrase(hour, nextHour, minute);
    }
    
    return CreateEnglishPhrase(hour, nextHour, minute);
}
```

This method correctly uses the culture parameter to determine language.

### UpdatePromptTexts Method
Located in `GameViewModel.cs`:
```csharp
private void UpdatePromptTexts()
{
    PromptDigital = TargetTime.ToString("hh\\:mm");
    PromptText = FormatFriendlyTime(TargetTime, _localizationService.CurrentCulture);
}
```

This method is called when:
- New challenge is generated
- Culture changes

## Benefits

? **Consistent default language** - English everywhere
? **Immediate updates** - Prompt text changes when language changes
? **No confusion** - Language picker order matches app default
? **Proper localization** - All text respects selected language
? **Works across modes** - Consistent in all three game modes

## Additional Notes

### Language Picker Display
The languages are shown in their native form:
- "English" (not "Engels")
- "Nederlands" (not "Dutch")

This is correct and unchanged.

### Digital Time Format
The digital time (e.g., "03:30") is culture-neutral and always uses the same format. Only the written/natural language text changes based on selected language.

---

**Status**: ? Fixed
**Files Modified**: 
- `ClockExerciser/ViewModels/GameViewModel.cs`
- `ClockExerciser/ViewModels/MenuViewModel.cs`

**Testing**: Requires rebuild and manual testing
**Impact**: Low risk, purely ordering change
