# Voice Features Implementation Plan

## Overview
Add comprehensive voice capabilities to Clock Exerciser for enhanced accessibility and child-friendly interaction. Both features use MAUI's built-in APIs and platform-native speech recognition with persistent user preferences.

---

## Core Requirements

### ?? Non-Negotiables
- **No paid services** - Use MAUI TextToSpeech + platform-native speech recognition
- **Separately configurable** - Independent toggles for input and output
- **Persistent settings** - User preferences saved across sessions
- **Default ON** - Both features enabled by default for new users
- **Bilingual** - Full support for English and Dutch
- **Regional accents** - US/GB for English, NL/BE for Dutch

### ? Simplified Approach (Phase 1)
- Use **MAUI's built-in `TextToSpeech.Default`** API (cross-platform, no custom code)
- Locale selection only (nl-BE, nl-NL, en-US, en-GB)
- Gender/voice actor selection **deferred** to future enhancement (platform-specific complexity)
- Focus on getting voice features working quickly across all platforms

---

## Voice Locale Selection Options

### English Voices
- ???? **American English** (en-US)
- ???? **British English** (en-GB)

### Dutch Voices
- ???? **Netherlands Dutch** (nl-NL)
- ???? **Belgian Dutch/Flemish** (nl-BE)

### Settings UI Includes:
1. **Voice Output Toggle** - Enable/disable TTS
2. **Voice Input Toggle** - Enable/disable speech recognition
3. **Region Picker** - Choose accent (US/GB for English, NL/BE for Dutch)
4. **Test Button** - Preview selected locale with sample phrase

### Platform Voice Availability

| Platform | Locale | Availability | Quality |
|----------|--------|--------------|---------|
| **Android** | en-US | ? High | Medium-High |
| | en-GB | ? High | Medium |
| | nl-NL | ? High | Medium |
| | nl-BE | ?? Medium (may fallback to nl-NL) | Low-Medium |
| **iOS** | en-US | ? High | High |
| | en-GB | ? High | High |
| | nl-NL | ? High | High |
| | nl-BE | ? High | High |
| **Windows** | en-US | ? High | Medium |
| | en-GB | ? High | Medium |
| | nl-NL | ? High | Medium |
| | nl-BE | ?? Low (may fallback) | Low |

---

## Feature 1: Voice Output (Text-to-Speech)

### Purpose
Speak prompts, feedback, and instructions to help younger users and accessibility.

### What to Speak
1. **Game Prompts**
   - "Set the clock to three fifteen" (Time to Clock mode)
   - "What time is shown on the clock?" (Clock to Time mode)
   
2. **Feedback Messages**
   - "Correct! Well done!" (success)
   - "Not quite right. Try again!" (incorrect)
   - "Great job! You scored [X] points"
   
3. **Instructions** (optional, on page load)
   - "Welcome to Clock Exerciser"

### Service Interface (Wrapper around MAUI API)
```csharp
public interface ITextToSpeechService
{
    Task SpeakAsync(string text, string? localeCode = null);
    Task<IEnumerable<LocaleInfo>> GetAvailableLocalesAsync();
    Task<bool> IsAvailableAsync();
    void Stop();
}

public class LocaleInfo
{
    public string Code { get; set; } // "en-US", "nl-BE"
    public string Language { get; set; } // "en", "nl"
    public string Country { get; set; } // "US", "BE"
    public string DisplayName { get; set; } // "???? American English"
}
```

### Implementation (Simple, Cross-Platform)
```csharp
public class TextToSpeechService : ITextToSpeechService
{
    public async Task SpeakAsync(string text, string? localeCode = null)
    {
        var options = new SpeechOptions();
        
        if (!string.IsNullOrEmpty(localeCode))
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            
            // Parse localeCode (e.g., "nl-BE")
            var parts = localeCode.Split('-');
            var language = parts[0];
            var country = parts.Length > 1 ? parts[1] : null;
            
            // Find exact match
            var locale = locales.FirstOrDefault(l => 
                l.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                (country == null || l.Country.Equals(country, StringComparison.OrdinalIgnoreCase)));
            
            // Fallback to language-only match
            if (locale == null)
            {
                locale = locales.FirstOrDefault(l => 
                    l.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
            }
            
            if (locale != null)
            {
                options.Locale = locale;
            }
        }
        
        await TextToSpeech.Default.SpeakAsync(text, options);
    }
    
    public async Task<IEnumerable<LocaleInfo>> GetAvailableLocalesAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        
        // Filter to supported languages and map to LocaleInfo
        return locales
            .Where(l => l.Language == "en" || l.Language == "nl")
            .Select(l => new LocaleInfo
            {
                Code = $"{l.Language}-{l.Country}",
                Language = l.Language,
                Country = l.Country,
                DisplayName = GetDisplayName(l.Language, l.Country)
            })
            .ToList();
    }
    
    public async Task<bool> IsAvailableAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        return locales?.Any() ?? false;
    }
    
    public void Stop()
    {
        // MAUI TextToSpeech doesn't have a Stop method in current version
        // May need platform-specific implementation if needed
    }
    
    private string GetDisplayName(string language, string country)
    {
        return (language, country) switch
        {
            ("en", "US") => "???? American English",
            ("en", "GB") => "???? British English",
            ("nl", "NL") => "???? Nederlands",
            ("nl", "BE") => "???? Vlaams",
            _ => $"{language}-{country}"
        };
    }
}
```

### Voice Fallback Logic (Built into Service)
```
1. Try: Exact locale match (e.g., nl-BE)
2. Fallback: Language-only match (nl-NL if nl-BE not available)
3. Fallback: System default
```

---

## Feature 2: Voice Input (Speech Recognition)

### Purpose
Allow users to speak their answers instead of typing (especially useful for children).

### What to Recognize
1. **Time values** (Clock to Time mode)
   - "Three fifteen" ? 3:15
   - "Kwart over drie" ? 3:15
   - "Half past two" ? 2:30
   - "Half drie" ? 2:30
   
2. **Natural language** - Reuse existing parsers
   - `EnglishTimeParser`
   - `DutchTimeParser`

### Service Interface
```csharp
public interface ISpeechRecognitionService
{
    Task<string?> RecognizeAsync(string locale = "en-US");
    Task<bool> RequestPermissionsAsync();
    Task<bool> IsAvailableAsync();
}
```

### Platform APIs (Still need platform-specific code)
- **Android**: `Android.Speech.SpeechRecognizer` + `RecognizerIntent`
- **iOS**: `Speech.SFSpeechRecognizer`
- **Windows**: `Windows.Media.SpeechRecognition.SpeechRecognizer`

**Note:** MAUI doesn't have built-in speech recognition, so this still requires platform-specific implementations.

---

## Feature 3: Settings Persistence

### Settings Service Interface (Simplified)
```csharp
public interface ISettingsService
{
    // Voice output settings
    Task<bool> GetVoiceOutputEnabledAsync();
    Task SetVoiceOutputEnabledAsync(bool enabled);
    
    // Voice input settings
    Task<bool> GetVoiceInputEnabledAsync();
    Task SetVoiceInputEnabledAsync(bool enabled);
    
    // Locale preference (simplified - no gender)
    Task<string> GetPreferredLocaleAsync(string language); // "en-US", "nl-BE"
    Task SetPreferredLocaleAsync(string language, string localeCode);
}
```

### Storage Schema (MAUI Preferences API)
```csharp
// Feature toggles
Preferences.Set("VoiceOutputEnabled", true);
Preferences.Set("VoiceInputEnabled", true);

// Per-language locale preferences (simplified)
Preferences.Set("PreferredLocale_en", "en-US"); // or "en-GB"
Preferences.Set("PreferredLocale_nl", "nl-NL"); // or "nl-BE"
```

### Default Values
```csharp
const bool DEFAULT_VOICE_OUTPUT = true;
const bool DEFAULT_VOICE_INPUT = true;
const string DEFAULT_LOCALE_EN = "en-US";
const string DEFAULT_LOCALE_NL = "nl-NL";
```

---

## Feature 4: Settings Page UI (Simplified)

### XAML Layout
```xml
<ContentPage Title="{Binding SettingsTitle}">
    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="20">
            
            <!-- Language Selection -->
            <Border Padding="15" StrokeShape="RoundRectangle 10">
                <VerticalStackLayout Spacing="10">
                    <Label Text="{Binding LanguageLabel}" FontSize="16" FontAttributes="Bold" />
                    <Picker ItemsSource="{Binding AvailableLanguages}"
                            SelectedItem="{Binding SelectedLanguage}" />
                </VerticalStackLayout>
            </Border>
            
            <!-- Voice Output Section -->
            <Border Padding="15" StrokeShape="RoundRectangle 10">
                <VerticalStackLayout Spacing="10">
                    <HorizontalStackLayout>
                        <Label Text="{Binding VoiceOutputLabel}" 
                               FontSize="16" FontAttributes="Bold"
                               HorizontalOptions="Start" />
                        <Switch IsToggled="{Binding VoiceOutputEnabled}" 
                                HorizontalOptions="End" />
                    </HorizontalStackLayout>
                    
                    <Label Text="{Binding VoiceOutputDescription}" 
                           FontSize="12" TextColor="{StaticResource Gray600}" />
                    
                    <!-- Locale Selection (visible when enabled) -->
                    <VerticalStackLayout IsVisible="{Binding VoiceOutputEnabled}" 
                                         Spacing="10" Padding="0,10,0,0">
                        
                        <Label Text="{Binding VoiceRegionLabel}" FontSize="14" FontAttributes="Bold" />
                        <Picker ItemsSource="{Binding AvailableRegions}"
                                ItemDisplayBinding="{Binding DisplayName}"
                                SelectedItem="{Binding SelectedRegion}" />
                        
                        <Button Text="{Binding TestVoiceButtonText}"
                                Command="{Binding TestVoiceCommand}"
                                BackgroundColor="{StaticResource Primary}" />
                    </VerticalStackLayout>
                </VerticalStackLayout>
            </Border>
            
            <!-- Voice Input Section -->
            <Border Padding="15" StrokeShape="RoundRectangle 10">
                <VerticalStackLayout Spacing="10">
                    <HorizontalStackLayout>
                        <Label Text="{Binding VoiceInputLabel}" 
                               FontSize="16" FontAttributes="Bold"
                               HorizontalOptions="Start" />
                        <Switch IsToggled="{Binding VoiceInputEnabled}" 
                                HorizontalOptions="End" />
                    </HorizontalStackLayout>
                    
                    <Label Text="{Binding VoiceInputDescription}" 
                           FontSize="12" TextColor="{StaticResource Gray600}" />
                </VerticalStackLayout>
            </Border>
            
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

### ViewModel Models (Simplified)
```csharp
public class RegionOption
{
    public string Code { get; set; } // "US", "GB", "NL", "BE"
    public string LocaleCode { get; set; } // "en-US", "nl-BE"
    public string DisplayName { get; set; } // "???? American", "???? Vlaams"
}
```

---

## Localization Strings (Simplified)

### English (AppResources.resx)
```xml
<!-- Settings Page -->
<data name="SettingsTitle"><value>Settings</value></data>
<data name="LanguageLabel"><value>Language</value></data>

<!-- Voice Output -->
<data name="VoiceOutputLabel"><value>Voice Output</value></data>
<data name="VoiceOutputDescription"><value>Speak prompts and feedback aloud</value></data>
<data name="VoiceRegionLabel"><value>Accent / Region</value></data>
<data name="TestVoiceButtonText"><value>?? Test Voice</value></data>
<data name="TestVoicePhrase"><value>Hello! This is how I will speak to you.</value></data>

<!-- Voice Input -->
<data name="VoiceInputLabel"><value>Voice Input</value></data>
<data name="VoiceInputDescription"><value>Speak your answers using the microphone</value></data>

<!-- TTS Feedback -->
<data name="TTSCorrect"><value>Correct! Well done!</value></data>
<data name="TTSIncorrect"><value>Not quite right. Try again!</value></data>
<data name="TTSScore"><value>Great job! You scored {0} points!</value></data>

<!-- Voice Input Feedback -->
<data name="Listening"><value>Listening...</value></data>
<data name="SpeechRecognitionFailed"><value>Could not understand. Please type your answer.</value></data>
<data name="MicrophonePermissionDenied"><value>Microphone permission required for voice input.</value></data>
```

### Dutch (AppResources.nl-NL.resx)
```xml
<!-- Settings Page -->
<data name="SettingsTitle"><value>Instellingen</value></data>
<data name="LanguageLabel"><value>Taal</value></data>

<!-- Voice Output -->
<data name="VoiceOutputLabel"><value>Spraakuitvoer</value></data>
<data name="VoiceOutputDescription"><value>Spreek aanwijzingen en feedback hardop uit</value></data>
<data name="VoiceRegionLabel"><value>Accent / Regio</value></data>
<data name="TestVoiceButtonText"><value>?? Test Stem</value></data>
<data name="TestVoicePhrase"><value>Hallo! Zo zal ik tegen je praten.</value></data>

<!-- Voice Input -->
<data name="VoiceInputLabel"><value>Spraakinvoer</value></data>
<data name="VoiceInputDescription"><value>Spreek uw antwoorden uit met de microfoon</value></data>

<!-- TTS Feedback -->
<data name="TTSCorrect"><value>Correct! Goed gedaan!</value></data>
<data name="TTSIncorrect"><value>Niet helemaal goed. Probeer het opnieuw!</value></data>
<data name="TTSScore"><value>Geweldig! Je hebt {0} punten gescoord!</value></data>

<!-- Voice Input Feedback -->
<data name="Listening"><value>Aan het luisteren...</value></data>
<data name="SpeechRecognitionFailed"><value>Kon u niet verstaan. Typ uw antwoord alstublieft.</value></data>
<data name="MicrophonePermissionDenied"><value>Microfoontoestemming vereist voor spraakinvoer.</value></data>
```

---

## Implementation Phases (Revised - Simpler!)

### Phase 1: Settings Infrastructure ?? 2-3 hours
1. Create `ISettingsService` interface (simplified)
2. Implement `SettingsService` with Preferences API
3. Register in DI container
4. Create `SettingsPage.xaml` + `SettingsViewModel`
5. Add locale selection UI (region picker only, no gender)
6. Add to Shell navigation

### Phase 2: Text-to-Speech ?? 2-3 hours
1. Create `ITextToSpeechService` interface
2. Implement `TextToSpeechService` using **MAUI's `TextToSpeech.Default`**
3. Implement locale discovery and fallback logic
4. Integrate into `SettingsViewModel` (test button)
5. Integrate into `GameViewModel` (speak prompts/feedback)
6. Test locale selection persistence

### Phase 3: Speech Recognition ?? 6-8 hours
1. Add platform permissions (AndroidManifest, Info.plist)
2. Create `ISpeechRecognitionService` interface
3. Implement Android speech recognition
4. Implement iOS speech recognition
5. Implement Windows speech recognition
6. Add microphone button to `GamePage`
7. Connect to time parsers
8. Test with various accents

### Phase 4: Polish & Testing ?? 2-3 hours
1. Visual feedback (listening indicator)
2. Error handling and fallbacks
3. Accessibility improvements
4. Performance optimization
5. Documentation updates

**Total Estimated Effort: 12-17 hours** (down from 16-22!)

---

## Files to Create (Simplified)

```
ClockExerciser/
??? Services/
?   ??? ISettingsService.cs
?   ??? SettingsService.cs
?   ??? ITextToSpeechService.cs (wrapper around MAUI API)
?   ??? TextToSpeechService.cs (cross-platform, no platform folders!)
?   ??? ISpeechRecognitionService.cs
??? Platforms/
?   ??? Android/
?   ?   ??? SpeechRecognitionService.cs (still needed)
?   ??? iOS/
?   ?   ??? SpeechRecognitionService.cs (still needed)
?   ??? Windows/
?       ??? SpeechRecognitionService.cs (still needed)
??? Pages/
?   ??? SettingsPage.xaml
?   ??? SettingsPage.xaml.cs
??? ViewModels/
?   ??? SettingsViewModel.cs
??? Models/
?   ??? LocaleInfo.cs
?   ??? RegionOption.cs
```

**Note:** No platform-specific TTS implementations needed! MAUI handles it.

## Files to Modify

```
- MauiProgram.cs (register services)
- AppShell.xaml (add Settings route)
- GameViewModel.cs (integrate TTS and SR)
- GamePage.xaml (add microphone button)
- AppResources.resx (add English strings)
- AppResources.nl-NL.resx (add Dutch translations)
- Platforms/Android/AndroidManifest.xml (RECORD_AUDIO permission)
- Platforms/iOS/Info.plist (speech recognition permission)
- Documents/PROJECT_PLAN.md (update with voice features)
- Documents/ARCHITECTURE.md (document voice services)
```

---

## Success Criteria (Updated)

? Voice output speaks prompts and feedback in correct language and accent  
? Voice input recognizes Dutch and English time phrases with regional variations  
? Settings persist across app restarts  
? Both features default to ON for new users  
? Both features can be independently toggled  
? Users can select accent/region (US/GB for English, NL/BE for Dutch)  
? Test voice button works correctly  
? Graceful fallback when preferred locale unavailable  
? No paid services or external APIs used  
? Works on Android, iOS, and Windows  

### Future Enhancements (Deferred)
?? Gender/voice actor selection (requires platform-specific implementations)  
?? Voice speed/pitch adjustment  
?? Advanced voice customization  

---

## Migration Path to Advanced Features

If gender/voice selection is needed later:

1. **Keep** existing `ITextToSpeechService` interface
2. **Add** `GetAvailableVoicesAsync()` method
3. **Create** platform-specific implementations:
   - `Platforms/Android/AdvancedTextToSpeechService.cs`
   - `Platforms/iOS/AdvancedTextToSpeechService.cs`
   - `Platforms/Windows/AdvancedTextToSpeechService.cs`
4. **Update** SettingsPage UI to show voice picker
5. **No breaking changes** - just enhanced functionality

---

**Status**: Planning Complete - Simplified Approach  
**Estimated Effort**: 12-17 hours (down from 16-22)  
**Dependencies**: Existing LocalizationService, Time Parsers, GameViewModel  
**Key Change**: Use MAUI's built-in TextToSpeech API instead of platform-specific code
