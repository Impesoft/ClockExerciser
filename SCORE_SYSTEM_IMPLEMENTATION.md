# Score System & Single Button UI - Implementation

## Overview
Implemented a unified UI with a single dynamic button and persistent score tracking, replacing the previous two-button design.

## Changes Implemented

### 1. Single Dynamic Button ?
**Before:**
- Two separate buttons: "Check Answer" and "Next Challenge"
- Both always visible

**After:**
- One button that changes based on state:
  - **"Check Answer"** - When challenge is active
  - **"Next Challenge"** - After correct answer
- User must answer correctly to proceed

### 2. Score Tracking ?
**Features:**
- Displays count of correct answers
- Shown as "? {count}" in top-right
- Increments automatically on correct answer
- Persisted to device storage (survives app restart)

### 3. Improved UX Flow ?
**Old Flow:**
1. User answers
2. Click "Check Answer"
3. See result
4. Can click "Next Challenge" regardless of correctness ?

**New Flow:**
1. User answers
2. Click "Check Answer"
3. See result
4. If **correct** ? Button changes to "Next Challenge" ?
5. If **incorrect** ? Must try again (button stays "Check Answer") ?

## Code Changes

### GameViewModel.cs

#### New Fields:
```csharp
private int _correctAnswers = 0;
private bool _answerChecked = false;
```

#### New Properties:
```csharp
public int CorrectAnswers
{
    get => _correctAnswers;
    private set
    {
        if (SetProperty(ref _correctAnswers, value))
        {
            // Persist score
            Preferences.Set("CorrectAnswers", value);
            OnPropertyChanged(nameof(ScoreText));
        }
    }
}

public string ScoreText => $"? {CorrectAnswers}";

public string PrimaryButtonText => _answerChecked && _resultSuccess 
    ? _localizationService.GetString("NextChallenge") 
    : _localizationService.GetString("SubmitAnswer");
```

#### Modified Methods:

**Constructor:**
```csharp
// Load saved score
_correctAnswers = Preferences.Get("CorrectAnswers", 0);
```

**ExecutePrimaryAction:** (Unified button logic)
```csharp
private void ExecutePrimaryAction()
{
    if (_answerChecked && _resultSuccess)
    {
        // User clicked "Next Challenge" after correct answer
        GenerateNewChallenge();
    }
    else
    {
        // User clicked "Check Answer"
        ExecuteCheckAnswer();
    }
}
```

**ExecuteCheckAnswer:**
```csharp
private void ExecuteCheckAnswer()
{
    // ... evaluation logic ...
    
    _answerChecked = true;
    OnPropertyChanged(nameof(PrimaryButtonText)); // Update button text
}
```

**GenerateNewChallenge:**
```csharp
public void GenerateNewChallenge()
{
    // ... reset logic ...
    _answerChecked = false; // Reset for new challenge
    OnPropertyChanged(nameof(PrimaryButtonText));
}
```

**SetResult:**
```csharp
private void SetResult(bool success, string? debugInfo = null)
{
    ResultSuccess = success;
    // ... message logic ...
    
    // Increment score on correct answer
    if (success)
    {
        CorrectAnswers++;
    }
    
    // ... audio feedback ...
}
```

### GamePage.xaml

#### Score Display (New):
```xaml
<!-- Score Display -->
<HorizontalStackLayout HorizontalOptions="End" Spacing="8">
    <Label Text="Score:" FontAttributes="Bold" VerticalOptions="Center"/>
    <Label Text="{Binding ScoreText}" 
           FontSize="20" 
           FontAttributes="Bold" 
           TextColor="#2e7d32"
           VerticalOptions="Center"/>
</HorizontalStackLayout>
```

#### Single Button (Replaced old two-button grid):
```xaml
<!-- Single Primary Action Button -->
<Button Text="{Binding PrimaryButtonText}"
        Command="{Binding CheckAnswerCommand}"
        IsEnabled="{Binding CanSubmit}"
        HorizontalOptions="Fill" />
```

## Persistence Details

### Storage:
- Uses `Preferences.Set("CorrectAnswers", value)`
- Built-in .NET MAUI cross-platform preferences
- Automatically persists to device storage

### Platform Storage Locations:
- **Android**: SharedPreferences
- **iOS**: NSUserDefaults
- **Windows**: ApplicationData.LocalSettings

### Data Lifetime:
- Survives app restarts ?
- Survives app updates (usually) ?
- Cleared on app uninstall ?
- Cleared on app data clear ?

## User Experience

### Correct Answer Flow:
1. User submits answer
2. ? Green success message
3. ?? Success sound plays
4. ? Score increments
5. Button changes to "Next Challenge"
6. User clicks to continue

### Incorrect Answer Flow:
1. User submits answer
2. ? Red error message
3. ?? Error sound plays
4. Score unchanged
5. Button stays "Check Answer"
6. User must try again

### Visual Feedback:
- **Score**: Green checkmark with count (? 5)
- **Success**: Green text + success sound
- **Failure**: Red text + error sound
- **Button**: Dynamic text changes based on state

## Testing Checklist

### Score Functionality:
- [ ] Score starts at 0 on first launch
- [ ] Score increments on correct answer
- [ ] Score doesn't change on incorrect answer
- [ ] Score persists after app restart
- [ ] Score visible in top-right corner

### Button Behavior:
- [ ] Shows "Check Answer" initially
- [ ] Shows "Next Challenge" after correct answer
- [ ] Stays "Check Answer" after incorrect answer
- [ ] Button enabled/disabled state works correctly
- [ ] Works in both Clock to Time and Time to Clock modes

### User Flow:
- [ ] Cannot skip to next challenge without correct answer
- [ ] Must keep trying until answer is correct
- [ ] Button text updates immediately after answer
- [ ] Score updates visually after correct answer
- [ ] All game modes work correctly

## Localization Support

### Required Strings:
Both `SubmitAnswer` and `NextChallenge` strings already exist in:
- `Resources/Strings/AppResources.resx` (English)
- `Resources/Strings/AppResources.nl-NL.resx` (Dutch)

No new localization strings needed! ?

## Future Enhancements

### Potential Additions:
1. **Score Reset Button**
   ```csharp
   public Command ResetScoreCommand => new Command(() => {
       CorrectAnswers = 0;
   });
   ```

2. **Show Incorrect Count**
   ```csharp
   private int _incorrectAnswers = 0;
   public string ScoreText => $"? {CorrectAnswers}  ? {_incorrectAnswers}";
   ```

3. **Accuracy Percentage**
   ```csharp
   public string AccuracyText => 
       $"{(double)CorrectAnswers / (CorrectAnswers + _incorrectAnswers):P0}";
   ```

4. **Score History**
   ```csharp
   // Track score per session
   public List<SessionScore> History { get; set; }
   ```

5. **Leaderboard**
   - Store high scores
   - Compare with friends
   - Daily/weekly challenges

## Benefits

### For Users:
? **Clearer flow** - One button to focus on
? **Motivation** - See score increasing
? **Progress tracking** - Score persists
? **Better learning** - Must get it right to proceed
? **Reduced clutter** - Simpler interface

### For Code:
? **Cleaner UI** - Less buttons, less confusion
? **Better state management** - Single truth source
? **Persistent progress** - Preferences API
? **Flexible** - Easy to add more score features

## Migration Notes

### For Existing Users:
- Score will start at 0 on first launch with this version
- No existing score to migrate (feature didn't exist before)
- All previous functionality preserved
- UI change is intuitive and self-explanatory

### For Developers:
- `NextChallengeCommand` removed (no longer needed)
- `CheckAnswerCommand` now handles both actions
- New `PrimaryButtonText` property for dynamic text
- New `CorrectAnswers` property for score
- Uses .NET MAUI `Preferences` API (cross-platform)

## Summary

### What Changed:
- ? Two buttons ? One dynamic button
- ? No score ? Persistent score counter
- ? Always can skip ? Must answer correctly
- ? Static UI ? Dynamic, state-driven UI

### Impact:
- **User Experience**: Much improved, more engaging
- **Code Quality**: Cleaner, better state management  
- **Learning**: Forces correct answers before proceeding
- **Motivation**: Score provides sense of achievement

---

**Status**: ? Implemented and ready for testing
**Breaking Changes**: None (UI change only)
**Migration Required**: No
**Testing Required**: Yes (see checklist above)
