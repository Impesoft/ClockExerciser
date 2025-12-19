# Clock Exerciser - Project Plan

## Project Overview
An educational .NET MAUI application to help users learn to read analog clocks and convert between analog and digital time formats. Supports both English and Dutch languages with natural language time parsing.

---

## Current State (Completed)

### ? Core Functionality
- Basic clock visualization with Syncfusion RadialGauge
- Three game modes: Clock to Time, Time to Clock, Random
- Bilingual support (English/Dutch)
- Time parsing for digital formats (HH:mm)
- Answer validation with visual feedback
- Proper 12-hour clock face with correct hand scaling
- Menu page with navigation to game modes
- Animated clock icon on menu page
- Native language display for language picker
- Full localization of menu and game pages

### ? Fixed Issues
- `TimeSpan.ToString("HH:mm")` format exception ? Changed to "hh:mm"
- Clock face showing 0 instead of 12 ? Added `LabelCreated` event handler
- Non-circular clock container ? Changed `Border.StrokeShape` to `Ellipse`
- Duplicate hour/minute labels ? Consolidated to single axis (0-12)
- Second hand completing rotation in 12 seconds ? Added `ConvertToDialValue` scaling
- Mode picker removed from GamePage ? Mode selection moved to MenuPage
- Default language changed to English ? App starts in en-US instead of nl-NL
- Menu items not translated ? Fixed Dutch translations in AppResources.nl-NL.resx
- Prompt text showing wrong language ? FormatFriendlyTime now respects current culture

---

## Phase 1: Application Structure & Navigation ?

### 1.1 Create Menu/Landing Page ?
- [x] Create `MenuPage.xaml` with app branding
- [x] Add large, playful buttons for:
  - "Clock to Time" mode
  - "Time to Clock" mode  
  - "Random Mode"
  - Settings/Language selection
- [x] Add app title and instructions
- [x] Implement navigation to `GamePage` (renamed from `MainPage`)
- [x] Replace placeholder icon with functional animated clock (3:15 display)
- [x] Show language names in native language (Nederlands, English)
- [x] Ensure full localization of menu items in both languages

**Implementation Notes:**
- MenuPage uses Syncfusion RadialGauge for animated clock display
- Navigation via Shell with mode passed as query parameter
- MenuViewModel handles culture changes reactively
- Default language changed to English (en-US)
- All menu text properly translated in Dutch resource file

### 1.2 Update Shell Navigation ?
- [x] Modify `AppShell.xaml` to register routes:
  - `///menu` (initial route)
  - `///game` (with navigation parameters for mode)
  - `///settings` (planned for future)
- [x] Pass selected game mode via `ShellNavigationQueryParameters`
- [x] Add back button to `GamePage` to return to menu
- [x] GameViewModel implements `IQueryAttributable` to receive navigation parameters

### 1.3 Refactor GamePage ?
- [x] Rename `MainPage.xaml` ? `GamePage.xaml`
- [x] Remove mode picker from game page (mode selected from menu)
- [x] Keep language picker on game page for quick switching
- [x] Update `MauiProgram.cs` and `AppShell.xaml` references
- [x] Remove `ModeOptions` and `SelectedMode` from `GameViewModel`
- [x] Simplify `GenerateNewChallenge()` to use mode from navigation

---

## Phase 2: Splash Screen & App Icon ?

### 2.1 Splash Screen ?
- [x] Design splash screen with playful clock illustration
- [x] Create `Resources/Splash/splash.svg`
- [x] Configure in ClockExerciser.csproj
- [x] Set appropriate base size (456x456)

**Implementation Notes:**
- Created SVG showing clock at 10:10 (smile time)
- Light blue background (#E3F2FD) matching app theme
- Simple, clean design with white clock face
- Blue hour markers at 12, 3, 6, 9 positions

### 2.2 App Icon ?
- [x] Design playful clock icon showing 3:15 time
- [x] Create `Resources/Images/appicon.svg` as scalable vector
- [x] Configure in ClockExerciser.csproj
- [x] Fixed duplicate resource issue by excluding from MauiImage wildcard

**Implementation Notes:**
- Blue circular background (#2196F3)
- White clock face with numbers at 12, 3, 6, 9
- Hour and minute hands showing 3:15
- Red second hand pointing at 12
- Removed old AppIcon folder to prevent conflicts

---

## Phase 3: Audio Feedback ?

### 3.1 Sound Assets ?
- [x] Create `Resources/Raw/` folder
- [x] Add `success.mp3` - Happy, encouraging sound ?
- [x] Add `error.mp3` - Gentle buzzer sound ?
- [x] Configure in `.csproj` (configured via `<MauiAsset Include="Resources\Raw\**" />`)

**Implementation Notes:**
- Audio files added: success.mp3 (23.5KB), error.mp3 (25.7KB)
- Files located in Resources/Raw/ folder
- Sounds are short (< 1 second) for quick feedback
- Format: MP3, suitable for mobile playback

### 3.2 Audio Service ?
- [x] Create `Services/IAudioService.cs` interface
- [x] Implement `Services/AudioService.cs`
- [x] Register in `MauiProgram.cs` DI container
- [x] Inject into `GameViewModel`
- [x] Call `PlaySuccessSound()` / `PlayErrorSound()` on answer validation
- [x] Add Plugin.Maui.Audio package for cross-platform audio
- [x] Implement actual audio playback

**Implementation Notes:**
- Uses Plugin.Maui.Audio (v3.0.1) for cross-platform audio
- IAudioManager registered in DI container
- AudioService loads files from app package using FileSystem
- Audio players created and disposed properly
- Integrated into GameViewModel.SetResult() method
- Ready for testing on all platforms

---

## Phase 4: Enhanced Time Parsing ?

### 4.1 Time Equivalence ?
- [x] Verify `MatchesTime` method handles 12/24 hour equivalence
  - 3:15 PM = 15:15 ?
  - 3:15 AM = 03:15 ?
- [x] Test edge cases (midnight, noon)

**Implementation Notes:**
- MatchesTime already correctly handles 12/24 hour conversion
- Checks time difference with +12h and -12h offsets
- 1-minute tolerance for matching

### 4.2 Dutch Natural Language Parser ?
- [x] Create `Services/DutchTimeParser.cs`
- [x] Parse "kwart over [uur]" ? :15 (e.g., "kwart over vijf" = 5:15)
- [x] Parse "half [uur]" ? :30 relative to next hour (e.g., "half vijf" = 4:30)
- [x] Parse "kwart voor [uur]" ? :45 (e.g., "kwart voor vier" = 3:45)
- [x] Parse "[minuten] over [uur]" ? hour:minutes (e.g., "tien over drie" = 3:10)
- [x] Parse "[minuten] voor [uur]" ? (hour-1):(60-minutes) (e.g., "tien voor vier" = 3:50)
- [x] Map Dutch hour names (een=1, twee=2, ..., twaalf=12)
- [x] Handle ambiguity (e.g., "vijf" as 5 or "five minutes")

**Implementation Notes:**
- Uses regex patterns for each Dutch time format
- Correctly implements "half vijf" = 4:30 (30 min before 5, not past 4)
- Handles both word and numeric minute forms
- Parse method returns TimeSpan? for clean null handling

### 4.3 English Natural Language Parser ?
- [x] Create `Services/EnglishTimeParser.cs`
- [x] Parse "quarter past [hour]" ? :15
- [x] Parse "half past [hour]" ? :30
- [x] Parse "quarter to [hour]" ? :45
- [x] Parse "[num] past [hour]" ? hour:num
- [x] Parse "[num] to [hour]" ? (hour-1):(60-num)
- [x] Parse "[hour] o'clock" ? hour:00
- [x] Map English hour names (one, two, ..., twelve)

**Implementation Notes:**
- Uses regex patterns for each English time format
- Supports hyphenated forms like "twenty-five past three"
- Handles both "o'clock" and "oclock"
- Consistent with DutchTimeParser architecture

### 4.4 Integration ?
- [x] Update `GameViewModel.TryParseUserTime()` to:
  1. Try digital formats (HH:mm, H:mm, h:mm) ?
  2. Try natural language based on current culture ?
  3. Return parsed `TimeSpan` or failure ?
- [x] Register parsers in DI container
- [x] Inject parsers into GameViewModel
- [ ] Add unit tests for parsers (deferred to Phase 6)

**Implementation Notes:**
- TryParseUserTime now attempts digital formats first for speed
- Falls back to culture-appropriate natural language parser
- Dutch users can type "kwart over vijf", English users "quarter past five"
- Parsers are singletons (stateless, reusable)

---

## Phase 5: UI/UX Improvements ??

### 5.1 Menu Page Design
- [ ] Use gradients or playful colors
- [ ] Add animations (e.g., clock hands spinning on load)
- [ ] Accessibility: ensure large touch targets, readable fonts

### 5.2 Game Page Enhancements
- [ ] Add visual feedback animations:
  - Green checkmark animation on correct answer
  - Red shake animation on incorrect answer
- [ ] Combine with audio feedback
- [ ] Add score tracking (optional)
- [ ] Timer mode (optional challenge)

### 5.3 Settings Page (Optional)
- [ ] Sound on/off toggle
- [ ] Difficulty selection (5-minute intervals vs. any time)
- [ ] Theme selection (light/dark)

---

## Phase 6: Testing & Quality Assurance ?

### 6.1 Unit Tests
- [ ] Create `ClockExerciser.Tests` project
- [ ] Test `DutchTimeParser`:
  - "kwart over vijf" = 5:15
  - "half vijf" = 4:30
  - "tien voor vier" = 3:50
- [ ] Test `EnglishTimeParser`:
  - "quarter past three" = 3:15
  - "ten to four" = 3:50
- [ ] Test `MatchesTime` for 12/24 hour equivalence
- [ ] Test `GameViewModel` logic

### 6.2 Manual Testing
- [ ] Test on Android emulator/device
- [ ] Test on iOS simulator/device
- [ ] Test on Windows
- [ ] Verify splash screen displays correctly
- [ ] Verify app icon appears in launcher
- [ ] Test audio on all platforms
- [ ] Test all natural language inputs (Dutch & English)
- [ ] Test navigation flow (menu ? game ? back)

---

## Phase 7: Deployment Preparation ??

### 7.1 Platform-Specific Configuration
- [x] **Android**: Code signing configured ?
  - Signing guide created (`Documents/ANDROID_SIGNING_GUIDE.md`)
  - CSPROJ updated with environment variable-based signing
  - .gitignore updated to exclude sensitive files
  - Helper scripts created:
    - `setup-android-signing.ps1` - One-time password setup
    - `build-android-release.ps1` - Interactive build script
  - Quick start guide: `ANDROID_BUILD_QUICKSTART.md`
- [x] Set package name: `com.clockexerciser.app` ?
- [x] Update version: 1.0.0 (version 1) ?
- [ ] **iOS**: Set bundle identifier, version, icons, splash screens
- [ ] **Windows**: Set app identity, publisher info

**Android Signing Setup:**
1. Generate keystore using keytool (see guides)
2. Run `.\setup-android-signing.ps1` to save passwords securely
3. Restart Visual Studio
4. Build release: `.\build-android-release.ps1` OR use Visual Studio Publish

**Security Features:**
- Passwords stored in Windows USER environment variables
- Not stored in project files or Git
- Helper scripts for easy setup and building
- Passwords never appear in command history

### 7.2 Localization Files ?
- [x] All strings in `AppResources.en.resx` / `AppResources.nl-NL.resx`
- [x] Menu page strings added
- [x] Game page strings complete

### 7.3 Documentation ?
- [x] `ANDROID_SIGNING_GUIDE.md` created with comprehensive instructions
- [x] `ANDROID_BUILD_QUICKSTART.md` created for quick reference
- [x] `README.md` created with:
  - App description
  - Supported languages
  - How to build/run
  - Project structure
  - Documentation links
- [x] `RELEASE_NOTES.md` created with version details and roadmap
- [x] `TODO.md` created with prioritized remaining work
- [ ] Add screenshots to `Documents/Screenshots/`

---

## Current Architecture

```
ClockExerciser/
??? App.xaml / App.xaml.cs
??? AppShell.xaml / AppShell.xaml.cs
??? MauiProgram.cs
??? Documents/
?   ??? PROJECT_PLAN.md (this file)
?   ??? ARCHITECTURE.md (to be created)
??? Helpers/
?   ??? ServiceHelper.cs
??? Models/
?   ??? GameMode.cs
?   ??? GameModeOption.cs
?   ??? LanguageOption.cs
??? Pages/
?   ??? MainPage.xaml (? rename to GamePage.xaml)
?   ??? MenuPage.xaml (to be created)
??? Services/
?   ??? LocalizationService.cs
?   ??? AudioService.cs (to be created)
?   ??? DutchTimeParser.cs (to be created)
?   ??? EnglishTimeParser.cs (to be created)
??? ViewModels/
?   ??? GameViewModel.cs
?   ??? MenuViewModel.cs (to be created)
??? Resources/
?   ??? Images/
?   ?   ??? appicon.svg (to be created)
?   ?   ??? splash.png (to be created)
?   ??? Raw/
?   ?   ??? success.mp3 (to be created)
?   ?   ??? error.mp3 (to be created)
?   ??? Strings/
?       ??? AppResources.resx
?       ??? AppResources.nl-NL.resx
```

---

## Dependencies
- **Syncfusion.Maui.Gauges** (already installed) - Clock visualization
- **.NET MAUI Community Toolkit** (consider adding) - Animations, converters
- **MediaElement** or platform audio APIs - Sound playback

---

## Next Steps
1. Start with **Phase 1.1**: Create `MenuPage.xaml`
2. Then **Phase 1.2**: Update shell navigation
3. Continue sequentially or prioritize based on user feedback

---

## Notes
- Keep existing functionality intact while refactoring
- Maintain bilingual support throughout all changes
- Test each phase before moving to the next
- Document breaking changes in this file

---

**Last Updated**: December 2024
**Status**: Phase 1-4 Complete, Phase 7 (Android) Complete

## Summary of Completion

### ? Phases Complete
- **Phase 1**: Application Structure & Navigation (100%)
- **Phase 2**: Splash Screen & App Icon (100%)
- **Phase 3**: Audio Feedback (100%) ? **NEW: Fully Complete!**
- **Phase 4**: Enhanced Time Parsing (100%)
- **Phase 7**: Android Code Signing (100%)

### ?? Phases In Progress / Remaining
- **Phase 5**: UI/UX Improvements (0% - animations, score tracking)
- **Phase 6**: Testing & Quality Assurance (0% - unit tests, manual testing)
- **Phase 7**: iOS/Windows Deployment (0% - pending)

### ?? Overall Progress: ~80%

**Core Features**: ? Complete and functional  
**Audio**: ? **NOW COMPLETE** with actual sound files and Plugin.Maui.Audio  
**Polish & Testing**: ?? Needs attention  
**Deployment**: ?? Android ready, ? iOS/Windows pending

### Priority Next Steps
1. ~~Add actual audio files~~ ? DONE!
2. Manual testing on Android device/emulator (test audio!)
3. Consider adding animations for better UX
4. Create unit tests for time parsers
5. ~~Update package name~~ ? DONE!

---

## Security & Local Development

### Local secrets (Syncfusion license key)
- [x] Move Syncfusion license key out of `MauiProgram.cs`
- [x] Load license key from `ClockExerciser/secrets.json` (git-ignored) or env var `SYNCFUSION_LICENSE_KEY`
- [x] Add `ClockExerciser/secrets.template.json` as a checked-in template
