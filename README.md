# Clock Exerciser

<div align="center">

![Clock Exerciser Logo](ClockExerciser/Resources/Images/appicon.svg)

**Master the art of telling time with Clock Exerciser**

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-10.0-512BD4)](https://dotnet.microsoft.com/apps/maui)
[![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS%20%7C%20Windows%20%7C%20macOS-brightgreen)](https://github.com/Impesoft/ClockExerciser)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

[Download](#download) • [Features](#features) • [Screenshots](#screenshots) • [Development](#development)

</div>

---

## ?? About

**Clock Exerciser** is an interactive educational app designed to help users of all ages learn to read analog clocks and convert between analog and digital time formats. With support for both English and Dutch languages, the app provides an engaging and effective way to master time-telling skills.

Whether you're helping children learn to read a clock for the first time or brushing up on your own skills, Clock Exerciser makes learning fun through gamification and immediate feedback.

## ? Features

### ?? Two Engaging Game Modes

- **Clock to Time**: Look at the analog clock and type the correct time
- **Time to Clock**: Read the digital/text time and set the analog clock hands correctly

### ?? Multilingual Support

- **English**: Full support with natural language time parsing
- **Dutch (Nederlands)**: Complete localization including special Dutch time phrases (e.g., "half vijf" = 4:30)

### ?? Learning Features

- **Interactive Clock Face**: Beautiful, smooth-animating analog clock with hour, minute, and second hands
- **Flexible Input**: Type time in digital format (3:15) or use natural language ("quarter past three", "kwart over drie")
- **Scoring System**: Track your progress with correct/wrong answer counters
- **High Score Tracking**: Beat your personal best and track it across sessions!
- **Game Over System**: Three strikes and you're out - try to get the highest score before making 3 mistakes!
- **Audio Feedback**: Immediate sound cues for correct and incorrect answers
- **Auto-Advance**: Automatically moves to the next challenge after a correct answer (1.5 second delay)
- **Smart Validation**: Answers within 1 minute tolerance are accepted as correct

### ?? Cross-Platform

Built with .NET MAUI, Clock Exerciser runs natively on:
- ?? **Android** (API 21+)
- ?? **iOS** (15.0+)
- ?? **Windows** (10.0.17763.0+)
- ?? **macOS** (Catalyst 15.0+)

## ?? How to Play

### Clock to Time Mode
1. Look at the analog clock showing a random time
2. Type the time you see in one of these formats:
   - **Digital**: `3:15`, `15:30`, `03:45`
   - **Natural Language (English)**: `quarter past three`, `ten to four`, `half past two`
   - **Natural Language (Dutch)**: `kwart over vijf`, `half vijf`, `tien voor vier`
3. Press **Check Answer** to see if you're correct
4. Auto-advances to the next challenge after 1.5 seconds on correct answers
5. Game ends after 3 wrong answers - your final score shows how many you got right!

### Time to Clock Mode
1. Read the time displayed in both digital format (e.g., `03:15`) and natural language (e.g., `quarter past three`)
2. Drag the **hour slider** (left/right) to set the correct hour
3. Drag the **minute slider** (left/right) to set the correct minutes
4. Press **Check Answer** to verify your answer
5. Auto-advances to the next challenge after 1.5 seconds on correct answers
6. Game ends after 3 wrong answers - try to beat your high score!

### Scoring
- ? **Correct Answer**: Score +1, auto-advance to next challenge
- ? **Wrong Answer**: Strike +1, manual retry on same challenge
- ?? **High Score**: Automatically saved and displayed
- ?? **Game Over**: After 3 strikes, see your final score and option to start a new game

## ?? Download

### Google Play Store
[Coming Soon]

### Apple App Store
[Coming Soon]

### Windows (Microsoft Store)
[Coming Soon]

### Build from Source
See [Development](#development) section below.

## ?? Screenshots

[Add screenshots here]

## ??? Technology Stack

- **Framework**: .NET MAUI 10.0
- **Language**: C# 14.0
- **UI Components**: 
  - Syncfusion.Maui.Gauges for clock visualization
  - Native MAUI controls for sliders and buttons
- **Architecture**: MVVM (Model-View-ViewModel)
- **Dependency Injection**: Built-in .NET DI container
- **Localization**: Resource-based system (.resx files)
- **Audio**: Platform-specific audio services with IAudioService interface
- **Navigation**: Shell-based navigation with routes
- **State Persistence**: Preferences API for high score tracking

## ??? Development

### Prerequisites

- Visual Studio 2022 (17.13+) or Visual Studio Code
- .NET 10 SDK
- Platform-specific workloads:
  - Android SDK (for Android development)
  - Xcode & iOS SDK (for iOS development - macOS only)
  - Windows SDK (for Windows development)

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/Impesoft/ClockExerciser.git
   cd ClockExerciser
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Run the app**
   
   For Android:
   ```bash
   dotnet build -t:Run -f net10.0-android
   ```
   
   For Windows:
   ```bash
   dotnet build -t:Run -f net10.0-windows10.0.19041.0
   ```
   
   For iOS (macOS only):
   ```bash
   dotnet build -t:Run -f net10.0-ios
   ```

### Project Structure

```
ClockExerciser/
??? Pages/              # XAML views
?   ??? MenuPage.xaml   # Main menu
?   ??? GamePage.xaml   # Game interface
??? ViewModels/         # ViewModels (MVVM pattern)
?   ??? MenuViewModel.cs
?   ??? GameViewModel.cs
??? Models/             # Data models and enums
?   ??? GameMode.cs
?   ??? LanguageOption.cs
??? Services/           # Business logic
?   ??? LocalizationService.cs
?   ??? IAudioService.cs
?   ??? DutchTimeParser.cs
?   ??? EnglishTimeParser.cs
??? Helpers/            # Utility classes
??? Converters/         # XAML value converters
?   ??? InvertedBoolConverter.cs
??? Resources/
    ??? Images/         # App icons, splash screens
    ??? Raw/            # Audio files (success/error sounds)
    ??? Strings/        # Localization resources
        ??? AppResources.resx (English)
        ??? AppResources.nl-NL.resx (Dutch)
```

### Key Components

- **GameViewModel**: Core game logic, scoring system, state management, and timer control
- **MenuViewModel**: Navigation to different game modes
- **LocalizationService**: Multi-language support with culture change events
- **DutchTimeParser** / **EnglishTimeParser**: Natural language time parsing with cultural awareness
- **IAudioService**: Platform-agnostic audio feedback system

### Architecture Patterns

- **MVVM**: Clean separation between UI and business logic
- **Dependency Injection**: Services registered in `MauiProgram.cs`
- **Data Binding**: Two-way binding for user input, one-way for display
- **Command Pattern**: All user interactions via `ICommand`
- **Observer Pattern**: `INotifyPropertyChanged` for reactive UI updates

## ?? Localization

### Supported Languages
- **English (en-US)**: Default language
- **Dutch (nl-NL)**: Full localization including special time phrases

### Adding a New Language

1. **Create resource file**: `Resources/Strings/AppResources.[culture].resx`
2. **Add all required keys** with translations:
   - AppTitle, LanguageLabel, SubmitAnswer, ResultCorrect, ResultIncorrect
   - ClockToTimeInstruction, TimeToClockInstruction
   - GameOver, NewGame, HighScore
   - All other UI strings
3. **Implement time parser** (if needed): Create `[Language]TimeParser.cs`
4. **Register language**: Add to `GameViewModel.Languages` collection
5. **Test thoroughly**: Verify UI layout with longer/shorter translations

### Dutch Time Logic
?? **Important**: Dutch time uses different logic than English for "half past"
- English: "half past three" = 3:30
- Dutch: "half drie" = 2:30 (30 minutes **before** drie/three)

This is handled in `DutchTimeParser.cs`.

## ?? Audio

The app uses audio feedback for:
- ? **Success sound**: Played when answer is correct
- ? **Error sound**: Played when answer is incorrect

Audio files should be placed in `Resources/Raw/` and accessed via `IAudioService`.

## ?? Testing

### Manual Testing Checklist
- [ ] Both game modes work correctly
- [ ] Language switching updates all UI strings
- [ ] Digital time input (e.g., "3:15") works
- [ ] Natural language input works in both languages
- [ ] Clock hand positioning is accurate
- [ ] Slider controls work smoothly
- [ ] Scoring system increments correctly
- [ ] High score persists across app restarts
- [ ] Game over triggers after 3 wrong answers
- [ ] Auto-advance works after correct answers
- [ ] Audio plays for correct/incorrect feedback
- [ ] Second hand ticks smoothly

### Platform Testing
Test on at least one platform before releasing:
- Android (emulator + real device)
- Windows
- iOS (if available)

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Contribution Guidelines
- Follow existing code style (see `.github/copilot-instructions.md`)
- Add unit tests for new parsers or validators
- Update localization for both English and Dutch
- Test on at least one platform
- Update README if adding new features

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ?? Author

**Impesoft**

- GitHub: [@Impesoft](https://github.com/Impesoft)
- Repository: [ClockExerciser](https://github.com/Impesoft/ClockExerciser)

## ?? Acknowledgments

- [Syncfusion](https://www.syncfusion.com/) for the excellent Gauges component
- The .NET MAUI team for the amazing cross-platform framework
- All contributors who help improve Clock Exerciser

## ?? Support

If you encounter any issues or have questions:

- ?? [Report a bug](https://github.com/Impesoft/ClockExerciser/issues)
- ?? [Request a feature](https://github.com/Impesoft/ClockExerciser/issues)
- ?? Contact: [Add your contact email]

---

<div align="center">

**Made with ?? using .NET MAUI**

? Star this repo if you find it helpful!

</div>
