# GitHub Copilot Instructions for Clock Exerciser Project

## Project Context
This is a .NET MAUI educational application called **Clock Exerciser** that helps users learn to read analog clocks and convert between analog and digital time formats. The app supports English and Dutch languages.

---

## ?? **IMPORTANT: Always Read Project Documentation First**

**Before starting ANY task**, read all files in the `Documents/` folder:
- `Documents/PROJECT_PLAN.md` - Current project plan, phases, and tasks
- `Documents/ARCHITECTURE.md` - Technical architecture and design decisions
- Any other `.md` files in the `Documents/` folder

This ensures you:
- ? Understand what has already been built
- ? Don't duplicate existing work
- ? Follow established patterns and conventions
- ? Continue from the correct project state
- ? Maintain consistency across the codebase

**WHEN COMPLETING TASKS:**
1. **Update `Documents/PROJECT_PLAN.md`**: Mark tasks as complete with `[x]` and add implementation notes
2. **Update `Documents/ARCHITECTURE.md`**: Document new components, patterns, and architectural decisions
3. Use `#file:'Documents/ARCHITECTURE.md'` reference when describing technical architecture
4. Use `#file:'Documents/PROJECT_PLAN.md'` reference when discussing project status or planning

---

## Technology Stack
- **.NET 10** (MAUI)
- **C# 14.0**
- **Platforms**: Android, iOS, Windows, MacCatalyst
- **UI Framework**: .NET MAUI with Syncfusion.Maui.Gauges for clock visualization

---

## Code Style & Conventions

### General Guidelines
- Follow C# naming conventions (PascalCase for types/methods, camelCase for fields)
- Use `var` for local variables when type is obvious
- Prefer expression-bodied members for simple properties/methods
- Use nullable reference types (`string?`) where appropriate
- Keep methods small and focused (Single Responsibility Principle)

### XAML Guidelines
- Use data binding instead of code-behind manipulation
- Prefer `{Binding}` syntax (already using MVVM)
- Use `{StaticResource}` for reusable styles
- Keep XAML clean: extract complex layouts to separate components if needed

### Architecture Patterns
- **MVVM**: All pages should have a corresponding ViewModel
- **Dependency Injection**: Use built-in .NET DI container
- **Services**: Business logic goes in service classes (not ViewModels)
- **Navigation**: Use Shell-based navigation with routes

---

## File Organization

```
ClockExerciser/
??? Documents/           # ?? READ FIRST - Project documentation
??? Pages/              # XAML views (to be created)
??? ViewModels/         # ViewModels (MVVM pattern)
??? Models/             # Data models and enums
??? Services/           # Business logic and utilities
??? Helpers/            # Static utility classes
??? Resources/
?   ??? Images/        # App icons, splash screens
?   ??? Raw/           # Audio files, assets
?   ??? Strings/       # Localization resources
```

---

## Localization

- Use `LocalizationService` for all user-facing strings
- Add keys to `Resources/Strings/AppResources.resx` (English)
- Add translations to `Resources/Strings/AppResources.nl-NL.resx` (Dutch)
- **Never hardcode UI strings in XAML or C#**

### **IMPORTANT: Localization Workflow**
**DO NOT attempt to add or modify resource strings yourself!**

When you need new localized strings:
1. **STOP** and list all required string keys with their English and Dutch values
2. **WAIT** for the user to add them to the resource files
3. Only continue implementation after user confirms strings are added

Example request format:
```
I need the following localization strings:

English (AppResources.resx):
- SettingsTitle = "Settings"
- VoiceOutputLabel = "Voice Output"

Dutch (AppResources.nl-NL.resx):
- SettingsTitle = "Instellingen"
- VoiceOutputLabel = "Spraakuitvoer"

Please add these strings and let me know when ready to continue.
```

Example usage (after strings are added):
```csharp
var title = _localizationService.GetString("AppTitle");
```

XAML binding:
```xaml
<Label Text="{Binding AppTitle}" />
```

---

## Clock-Specific Logic

### Clock Hand Scaling
- Hour hand: Direct 0-12 value
- Minute/Second hands: Use `ConvertToDialValue(rawValue)` which divides by 5
- The gauge axis is 0-12 with 4 minor ticks per interval (60 total subdivisions)

### Time Validation
- Use `MatchesTime()` for comparing times with tolerance
- Use `CircularDifference()` for angular comparisons (handles wrap-around)
- Account for 12/24-hour equivalence (3:15 PM = 15:15)

### Avoid These Mistakes
- ? Don't use `TimeSpan.ToString("HH:mm")` ? Use `"hh:mm"` instead
- ? Don't create multiple gauge axes for hours/minutes ? Use single 0-12 axis
- ? Don't pass raw 0-59 values to minute/second pointers ? Scale them first

---

## Testing Requirements

When adding new features:
1. Write unit tests for business logic (parsers, validators, calculations)
2. Manual test on at least one platform (Windows/Android)
3. Verify localization works in both English and Dutch
4. Test navigation flow (if applicable)

---

## Audio Integration

When implementing audio:
- Use async methods (`Task PlaySuccessSound()`)
- Don't block the UI thread
- Provide an option to mute (future settings page)
- Use platform-specific audio APIs or MediaElement

---

## Natural Language Parsing

When implementing time parsers:
- Support both exact phrases and flexible input
- Handle ambiguity (e.g., Dutch "vijf" = 5 or "five minutes")
- Return `TimeSpan?` (null on parse failure)
- Test with edge cases (midnight, noon, "half past" logic)

### Dutch Time Logic
- "half [uur]" = 30 minutes **before** the hour (e.g., "half vijf" = 4:30, not 5:30)
- This is different from English "half past"

---

## Common Tasks

### Adding a New Page
1. Create `Pages/YourPage.xaml` and `Pages/YourPage.xaml.cs`
2. Create `ViewModels/YourViewModel.cs`
3. Register both in `MauiProgram.cs`:
   ```csharp
   builder.Services.AddTransient<YourPage>();
   builder.Services.AddTransient<YourViewModel>();
   ```
4. Add route in `AppShell.xaml`:
   ```xml
   <ShellContent Route="yourpage" ContentTemplate="{DataTemplate pages:YourPage}" />
   ```

### Adding a New Service
1. Create interface `Services/IYourService.cs`
2. Create implementation `Services/YourService.cs`
3. Register in `MauiProgram.cs`:
   ```csharp
   builder.Services.AddSingleton<IYourService, YourService>();
   ```
4. Inject into ViewModels via constructor

### Adding Localized Strings
1. Add to `AppResources.resx`: key="YourKey", value="English text"
2. Add to `AppResources.nl-NL.resx`: key="YourKey", value="Dutch translation"
3. Access via `_localizationService.GetString("YourKey")`

---

## Platform-Specific Notes

### Android
- Test audio on real device (emulator audio can be unreliable)
- Ensure app icon displays correctly in launcher
- Check permissions in `AndroidManifest.xml` if needed

### iOS
- Test splash screen timing (may need adjustment)
- Verify app icon for all sizes (App Store requirements)
- Test on both iPhone and iPad if possible

### Windows
- Ensure clock rendering is smooth (GPU acceleration)
- Test keyboard navigation (Enter to submit, Tab between controls)

---

## Git Workflow

- Commit frequently with clear messages
- Reference tasks from PROJECT_PLAN.md in commits
- Update PROJECT_PLAN.md checkboxes as tasks complete
- Update ARCHITECTURE.md if design changes

---

## Documentation Maintenance

When completing a task:
1. **Update PROJECT_PLAN.md**: Mark task as complete (`[x]`)
2. **Update ARCHITECTURE.md**: Add new components, update diagrams
3. **Add notes in PROJECT_PLAN.md**: Document any deviations or decisions

When starting a new phase:
1. **Review PROJECT_PLAN.md**: Understand dependencies
2. **Check ARCHITECTURE.md**: Understand current structure
3. **Ask for clarification** if anything is unclear

---

## Communication Style

When presenting changes:
- ? Be concise and direct
- ? Show code snippets for clarity
- ? Explain *why* a decision was made (not just *what* changed)
- ? Offer alternatives if multiple approaches exist
- ? Avoid long explanations unless requested
- ? Don't ask for permission to use tools (just use them)

---

## Quick Reference

| Task | Command/Pattern |
|------|-----------------|
| Read project docs | Check `Documents/*.md` **first** |
| Add localized string | AppResources.resx + AppResources.nl-NL.resx |
| Register service | `builder.Services.Add*<Interface, Implementation>()` |
| Navigate | `await Shell.Current.GoToAsync("///route")` |
| Play audio | `await _audioService.PlaySuccessSound()` |
| Parse Dutch time | `_dutchTimeParser.Parse("kwart over vijf")` |
| Scale clock value | `ConvertToDialValue(minutes)` |

---

**Last Updated**: [Current Date]
**Project Version**: 0.1.0-alpha
