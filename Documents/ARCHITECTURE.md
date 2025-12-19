# Clock Exerciser - Architecture Documentation

## Overview
This document describes the technical architecture of the Clock Exerciser application, a .NET MAUI educational app for learning to read analog clocks.

---

## Technology Stack

### Framework
- **.NET 10** (MAUI)
- **C# 14.0**
- **Target Platforms**: Android, iOS, Windows, MacCatalyst

### Key Libraries
- **Syncfusion.Maui.Gauges** (v32.1.19) - Radial gauge for clock face
- **.NET MAUI** - Cross-platform UI framework

### Patterns & Practices
- **MVVM** (Model-View-ViewModel)
- **Dependency Injection** (built-in .NET DI)
- **Service-oriented architecture**
- **Shell navigation**

---

## Project Structure

### 1. Core Application Layer

#### `App.xaml / App.xaml.cs`
- Application entry point
- Defines global resources and styles

#### `AppShell.xaml / AppShell.xaml.cs`
- Shell-based navigation container
- Route registration
- Navigation bar configuration

#### `MauiProgram.cs`
- DI container configuration
- Service registration
- Platform-specific initialization

---

### 2. Pages (Views)

#### `GamePage.xaml` (formerly `MainPage.xaml`) ?
**Current Responsibilities:**
- Display analog clock using `SfRadialGauge`
- Show language picker for quick switching
- Display instruction text based on game mode
- Input controls (Entry for typing, Sliders for hand adjustment)
- Answer validation UI
- Success/error result display

**Binding Context:** `GameViewModel`

**Recent Changes:**
- Renamed from MainPage to GamePage
- Mode picker removed (mode now comes from navigation)
- Implements `IQueryAttributable` to receive mode parameter
- Clock border changed to ellipse for circular appearance
- Proper 12-hour face with 0?12 label conversion

#### `MenuPage.xaml` ? **NEW**
**Current Responsibilities:**
- Display app title and subtitle (localized)
- Show animated clock icon using Syncfusion RadialGauge (displays 3:15)
- Three mode selection buttons (Clock to Time, Time to Clock, Random)
- Language picker showing native language names
- Navigate to GamePage with selected mode

**Binding Context:** `MenuViewModel`

**Implementation Details:**
- Shell.NavBarIsVisible="False" for full-screen menu
- Colorful buttons with different colors per mode (blue, green, orange)
- Circular clock border with themed colors
- All text fully localized and updates reactively

---

### 3. ViewModels

#### `GameViewModel.cs` ?
**Current Responsibilities:**
- Manage game state (mode, target time, user input)
- Generate random time challenges
- Validate user answers (text input or clock hands)
- Provide localized strings
- Expose pointer values for clock hands
- Calculate circular differences for validation
- **Receive game mode via navigation parameters** (implements `IQueryAttributable`)

**Key Properties:**
- `IsClockToTime`, `IsTimeToClock` - Mode flags
- `TargetTime` - Generated challenge time
- `HourPointerValue`, `MinutePointerValue`, `SecondPointerValue` - Clock hand positions
- `UserHourValue`, `UserMinuteValue`, `UserSecondValue` - User's hand adjustments
- `AnswerText` - User's text input
- `ResultVisible`, `ResultSuccess`, `ResultMessage` - Feedback UI

**Key Methods:**
- `GenerateNewChallenge()` - Creates new time challenge
- `ExecuteCheckAnswer()` - Validates user answer
- `TryParseUserTime()` - Parses digital time input
- `MatchesTime()` - Compares times with tolerance
- `ConvertToDialValue()` - Scales 0-59 values to 0-12 gauge
- `ApplyQueryAttributes()` - Receives mode from navigation

**Recent Changes:**
- Removed `ModeOptions` and `SelectedMode` properties
- Added `IQueryAttributable` interface for navigation parameters
- Simplified `GenerateNewChallenge()` logic
- Removed `EvaluateCultureLists()` method (no longer needed)

#### `MenuViewModel.cs` ? **NEW**
**Current Responsibilities:**
- Handle mode selection via commands
- Navigate to GamePage with mode parameter
- Expose language options with native names
- Provide localized menu strings

**Key Properties:**
- `Languages` - Collection of LanguageOption (Nederlands, English)
- `SelectedLanguage` - Currently selected language
- `ClockToTimeButton`, `TimeToClockButton`, `RandomModeButton` - Localized button text
- `MenuTitle`, `MenuSubtitle` - Localized menu titles
- `LanguageLabel` - Localized "Language" label

**Key Commands:**
- `ClockToTimeCommand` - Navigates to game with ClockToTime mode
- `TimeToClockCommand` - Navigates to game with TimeToClock mode
- `RandomModeCommand` - Navigates to game with Random mode

**Implementation:**
- Uses Shell navigation with query parameters
- Subscribes to `CultureChanged` event for reactive localization
- Updates all bound properties when language changes

---

### 4. Models

#### `GameMode.cs`
```csharp
public enum GameMode
{
    ClockToTime,  // User reads clock, types time
    TimeToClock,  // User reads time, sets clock hands
    Random        // Alternates between modes
}
```

#### `GameModeOption.cs`
- Wrapper for `GameMode` with localized label
- Implements `INotifyPropertyChanged`
- `UpdateLabel(Func<string, string> translator)` for re-localization

#### `LanguageOption.cs` ?
- Wraps culture code (e.g., "nl-NL", "en-US")
- Displays language name in **native language** (Nederlands, English)
- Implements `INotifyPropertyChanged`
- No longer uses resource key translation (simplified)

**Recent Changes:**
- Removed `UpdateDisplay()` method
- Removed `ResourceKey` property
- Constructor now takes `nativeName` directly
- Languages always display in their native form

---

### 5. Services

#### `LocalizationService.cs` ?
**Current Responsibilities:**
- Manage `CurrentCulture` (thread culture + UI culture)
- Expose `CultureChanged` event
- Provide `GetString(key)` for resource lookup
- **Default to en-US culture**

**Usage:**
```csharp
_localizationService.SetCulture(new CultureInfo("nl-NL"));
var text = _localizationService.GetString("AppTitle");
```

**Resource Files:**
- `Resources/Strings/AppResources.en.resx` (English, default)
- `Resources/Strings/AppResources.nl-NL.resx` (Dutch)

**Recent Changes:**
- Default culture changed from `nl-NL` to `en-US`
- Ensures app starts in English by default
- All resource keys properly translated in both files

#### `AudioService.cs` ?
**Current Responsibilities:**
- Play success sound when user answers correctly
- Play error sound when user answers incorrectly
- Load audio files from app package
- Manage audio player lifecycle

**Interface:**
```csharp
public interface IAudioService
{
    Task PlaySuccessSound();
    Task PlayErrorSound();
}
```

**Implementation:**
- ? Uses Plugin.Maui.Audio for cross-platform playback
- ? Loads MP3 files from Resources/Raw folder
- ? Creates and disposes audio players properly
- ? Integrated into GameViewModel
- ? Error handling with debug logging

**Dependencies:**
- Plugin.Maui.Audio (v3.0.1)
- IAudioManager (injected via DI)

**Audio Files:**
- `success.mp3` - 23.5 KB
- `error.mp3` - 25.7 KB

---

#### `DutchTimeParser.cs` ? **NEW**
**Responsibilities:**
- Parse Dutch natural language time expressions
- Support patterns: kwart over/voor, half, [minuten] over/voor, [uur] uur
- Return TimeSpan? for successful parse or null

**Examples:**
- "kwart over vijf" ? 5:15
- "half vijf" ? 4:30 (30 minutes BEFORE five, Dutch convention)
- "tien voor vier" ? 3:50
- "vijf uur" ? 5:00

**Implementation:**
- Regex-based pattern matching
- Dictionary mapping Dutch hour words to integers
- Handles ambiguity (e.g., "vijf" can be 5 or 5 minutes)

#### `EnglishTimeParser.cs` ? **NEW**
**Responsibilities:**
- Parse English natural language time expressions
- Support patterns: quarter past/to, half past, [num] past/to, o'clock
- Return TimeSpan? for successful parse or null

**Examples:**
- "quarter past three" ? 3:15
- "half past two" ? 2:30
- "ten to four" ? 3:50
- "five o'clock" ? 5:00

**Implementation:**
- Regex-based pattern matching
- Dictionary mapping English hour and minute words
- Supports hyphenated forms ("twenty-five past three")

---

### 6. Helpers

#### `ServiceHelper.cs`
Utility for accessing DI services from non-injected contexts (e.g., XAML code-behind).

```csharp
public static class ServiceHelper
{
    public static IServiceProvider? Services { get; set; }
    
    public static T GetRequiredService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }
}
```

**Usage:**
```csharp
BindingContext = ServiceHelper.GetRequiredService<GameViewModel>();
```

---

## Data Flow

### Clock-to-Time Mode
1. `GameViewModel.GenerateNewChallenge()` creates random `TargetTime`
2. `TargetTime` updates `HourPointerValue`, `MinutePointerValue`, `SecondPointerValue`
3. Clock hands render via bindings to `SfRadialGauge.NeedlePointer.Value`
4. User types time in `Entry` ? bound to `AnswerText`
5. User clicks "Submit" ? `CheckAnswerCommand` ? `ExecuteCheckAnswer()`
6. `TryParseUserTime()` parses input
7. `MatchesTime()` validates with tolerance
8. `ResultSuccess`, `ResultMessage` update ? UI shows feedback

### Time-to-Clock Mode
1. `GameViewModel.GenerateNewChallenge()` creates `TargetTime`
2. `PromptText` and `PromptDigital` display the time to set
3. User adjusts `Slider` controls ? bound to `UserHourValue`, `UserMinuteValue`, `UserSecondValue`
4. Values update `HourPointerValue`, `MinutePointerValue`, `SecondPointerValue` (via computed properties)
5. Clock hands move in real-time
6. User clicks "Submit" ? `CheckAnswerCommand` ? `EvaluateClockAnswer()`
7. `CircularDifference()` calculates angular error for each hand
8. Success if all hands within tolerance

---

## Clock Rendering Details

### Syncfusion RadialGauge Configuration
- **Single `RadialAxis`** from 0 to 12
- **StartAngle**: 270° (12 o'clock at top)
- **EndAngle**: 270° (full circle)
- **ShowLastLabel**: False (prevents duplicate 12)
- **LabelCreated Event**: Converts "0" label to "12"
- **MinorTicksPerInterval**: 4 (60 total subdivisions for minutes/seconds)

### Pointer Scaling
- **Hour Hand**: Raw value 0-12 (directly maps to gauge)
- **Minute/Second Hands**: Raw value 0-59, scaled via `ConvertToDialValue(v) => v / 5.0`
  - Maps 0-59 to 0-11.8 for gauge display
  - Evaluation logic still uses 0-59 for accuracy

### Hand Calculations
```csharp
// Hour hand includes fractional movement from minutes/seconds
HourPointerValue = hours + minutes/60 + seconds/3600

// Minute hand includes fractional movement from seconds
MinutePointerValue = ConvertToDialValue(minutes + seconds/60)

// Second hand
SecondPointerValue = ConvertToDialValue(seconds)
```

---

## Navigation Flow (Implemented)

```
[MenuPage] ??? (select mode) ??? [GamePage]
     ?                                 ?
(language picker)               (back button)
     ?                                 ?
(updates all text)              (returns to menu)
```

### Shell Routes (Current)
- `///menu` - MenuPage (initial route) ?
- `///game` - GamePage (receives mode parameter) ?
- `///settings` - Settings page (planned for future)

### Navigation Implementation
```csharp
// MenuViewModel navigates to game with mode parameter
await Shell.Current.GoToAsync("///game", new Dictionary<string, object>
{
    { "mode", GameMode.ClockToTime }
});

// GameViewModel receives mode via IQueryAttributable
public void ApplyQueryAttributes(IDictionary<string, object> query)
{
    if (query.TryGetValue("mode", out var modeObj) && modeObj is GameMode mode)
    {
        _activeMode = mode;
        GenerateNewChallenge();
    }
}
```

---

## Dependency Injection Setup

### `MauiProgram.cs` ?
```csharp
builder.Services.AddSingleton(AudioManager.Current);
builder.Services.AddSingleton<LocalizationService>();
builder.Services.AddSingleton<IAudioService, AudioService>();
builder.Services.AddSingleton<DutchTimeParser>();
builder.Services.AddSingleton<EnglishTimeParser>();
builder.Services.AddTransient<GameViewModel>();
builder.Services.AddTransient<MenuViewModel>();
builder.Services.AddTransient<GamePage>();
builder.Services.AddTransient<MenuPage>();
```

**Recent Changes:**
- All Phase 3 and 4 services now registered
- AudioManager.Current registered for Plugin.Maui.Audio
- AudioService fully implemented with actual audio playback
- Time parsers available for natural language input
- All pages and ViewModels registered for DI

---

## Current Architecture

```
ClockExerciser/
??? App.xaml / App.xaml.cs
??? AppShell.xaml / AppShell.xaml.cs
??? MauiProgram.cs
??? Documents/
?   ??? PROJECT_PLAN.md
?   ??? ARCHITECTURE.md (this file)
??? Helpers/
?   ??? ServiceHelper.cs
??? Models/
?   ??? GameMode.cs
?   ??? GameModeOption.cs
?   ??? LanguageOption.cs
??? Pages/
?   ??? GamePage.xaml / GamePage.xaml.cs (renamed from MainPage)
?   ??? MenuPage.xaml / MenuPage.xaml.cs ?
??? Services/
?   ??? LocalizationService.cs ?
?   ??? AudioService.cs ? (Phase 3 - infrastructure complete)
?   ??? DutchTimeParser.cs ? (Phase 4)
?   ??? EnglishTimeParser.cs ? (Phase 4)
??? ViewModels/
?   ??? GameViewModel.cs
?   ??? MenuViewModel.cs ?
??? Resources/
?   ??? Images/
?   ?   ??? appicon.svg ?
?   ?   ??? dotnet_bot.png
?   ??? Splash/
?   ?   ??? splash.svg ?
?   ??? Fonts/
?   ?   ??? OpenSans-Regular.ttf
?   ?   ??? OpenSans-Semibold.ttf
?   ??? Raw/
?   ?   ??? success.mp3 ?
?   ?   ??? error.mp3 ?
?   ?   ??? README.txt
?   ??? Strings/
?       ??? AppResources.en.resx
?       ??? AppResources.nl-NL.resx
```

---

## Visual Assets (Phase 2) ?

### App Icon
- **Location**: `Resources/Images/appicon.svg`
- **Design**: Blue circular background with white clock face
- **Time Shown**: 3:15 (hour and minute hands)
- **Features**: 
  - Numbers at 12, 3, 6, 9 positions
  - Red second hand at 12
  - Professional, clean design
- **Configuration**: `<MauiIcon>` in CSPROJ with #2196F3 color
- **Platforms**: Android, iOS, Windows, macOS (auto-scaled)

### Splash Screen
- **Location**: `Resources/Splash/splash.svg`
- **Design**: Light blue background with white clock face
- **Time Shown**: 10:10 (traditional "smile time")
- **Features**:
  - Blue hour markers at cardinal positions
  - Two hands forming pleasant angle
  - Simple, minimalist design
- **Configuration**: `<MauiSplashScreen>` with #E3F2FD background, 456x456 base size
- **Platforms**: Android, iOS, Windows (auto-generated per platform)

---

## Known Issues & Technical Debt

### Fixed ?
- ? TimeSpan format string (HH ? hh)
- ? Clock face showing 0 (LabelCreated event)
- ? Non-circular border (Ellipse shape)
- ? Overlapping hour/minute labels (single axis)
- ? Second hand rotating too fast (scaling logic)
- ? Mode picker on game page (moved to MenuPage)
- ? Menu navigation structure implemented
- ? MenuPage with animated clock icon
- ? Language names showing in wrong language (now native)
- ? Default language was Dutch (changed to English)
- ? Menu items not translated (fixed Dutch resources)
- ? Prompt text not translating (fixed FormatFriendlyTime)

### To Address
- [x] Natural language parsing ? (Phase 4 - Complete)
- [x] Audio feedback ? (Phase 3 - Complete with actual audio files)
- [x] Splash screen and custom icon ? (Phase 2 - Complete)
- [ ] Settings page for preferences (Phase 5)
- [ ] Score tracking or timer mode (Phase 5)
- [ ] Unit tests (Phase 6)
- [x] Android code signing ? (Phase 7 - Complete)

---

## Code Signing & Deployment

### Android Signing ?
- **Status**: Configured and ready
- **Documentation**: `Documents/ANDROID_SIGNING_GUIDE.md`
- **Keystore**: User needs to generate using `keytool`
- **Configuration**: CSPROJ updated with Release signing
- **Build Command**: `dotnet build -c Release -f net10.0-android`
- **Publish AAB**: `dotnet publish -c Release -f net10.0-android -p:AndroidPackageFormat=aab`

**Key Files**:
- `ClockExerciser/ClockExerciser.csproj` - Signing configuration
- `ClockExerciser/signing.keystore` - Keystore file (not in Git)
- `ClockExerciser/signing.properties.template` - Configuration template
- `.gitignore` - Excludes sensitive signing files

**Security**:
- Keystore and passwords excluded from Git
- Passwords prompted during build (not hardcoded)
- Template file provided for reference

### iOS Signing (Future)
- Requires Apple Developer Account ($99/year)
- Provisioning profiles needed
- Code signing certificate required

### Windows Signing (Future)
- Self-signed certificate for testing
- Microsoft Store requires publisher certificate

---

## Performance Considerations

- **Clock Updates**: Bound properties update on user input ? smooth performance with MAUI's property change notifications
- **Random Time Generation**: Minimal overhead (`Random.Next()` calls)
- **Audio Playback**: Use async methods to avoid blocking UI
- **Image Resources**: Use SVG for scalable icons (smaller file size, better quality)

---

## Local configuration / secrets
- The Syncfusion license key is not stored in source.
- At startup, `MauiProgram.cs` loads `secrets.json` (optional) from the app content root and reads `Syncfusion:LicenseKey`.
- Alternatively, set environment variable `SYNCFUSION_LICENSE_KEY`.
- `ClockExerciser/secrets.json` is git-ignored; `ClockExerciser/secrets.template.json` is included as a safe template.
