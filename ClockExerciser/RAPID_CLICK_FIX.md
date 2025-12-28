# Rapid-Fire Button Spam Fix

## The Bug ??

**Issue**: Users could rapidly click/tap the "Controleren" (Check Answer) button multiple times in quick succession, allowing them to:
- Submit the same answer multiple times
- Potentially score points multiple times for a single challenge
- Cheat the game by spamming the button

## Root Cause

The `CheckAnswerCommand` had no guard against rapid submissions. Between the time the button was pressed and the answer was evaluated, users could press it again (and again, and again...).

## The Fix ?

### 1. Added State Flag
```csharp
private bool _isCheckingAnswer = false;
```

### 2. Updated `CanSubmit` Property
```csharp
public bool CanSubmit => !_isCheckingAnswer && 
    (IsClockToTime ? !string.IsNullOrWhiteSpace(AnswerText) : true);
```

Now the button is disabled when:
- Answer is being checked (`_isCheckingAnswer == true`)
- OR in ClockToTime mode with empty text input

### 3. Disable Button Immediately on Click
```csharp
private void ExecutePrimaryAction()
{
    if (IsGameOver || _isCheckingAnswer)
    {
        return; // Extra safety check
    }
    
    // Disable button IMMEDIATELY
    _isCheckingAnswer = true;
    OnPropertyChanged(nameof(CanSubmit));
    
    ExecuteCheckAnswer();
}
```

### 4. Re-enable Button Only on New Challenge
```csharp
public void GenerateNewChallenge()
{
    // ... generate new challenge ...
    
    // Re-enable the check button for the new challenge
    _isCheckingAnswer = false;
    
    // ... notify UI ...
    OnPropertyChanged(nameof(CanSubmit));
}
```

## Flow Diagram

```
User clicks "Controleren"
    ?
ExecutePrimaryAction() called
    ?
Set _isCheckingAnswer = true ? Button DISABLED
    ?
Notify UI (OnPropertyChanged)
    ?
ExecuteCheckAnswer() evaluates answer
    ?
If correct: Wait 1.5s ? GenerateNewChallenge()
    ?
Set _isCheckingAnswer = false ? Button RE-ENABLED
    ?
Notify UI (OnPropertyChanged)
    ?
New challenge ready, button clickable again
```

## What Happens on Spam Click?

**Before Fix:**
```
Click 1: Evaluate answer ? +1 score
Click 2: Evaluate answer ? +1 score (CHEAT!)
Click 3: Evaluate answer ? +1 score (CHEAT!)
Result: 3 points for 1 challenge
```

**After Fix:**
```
Click 1: Evaluate answer ? Button DISABLED ? +1 score
Click 2: IGNORED (CanSubmit = false)
Click 3: IGNORED (CanSubmit = false)
...wait 1.5s...
New challenge ? Button RE-ENABLED
Result: 1 point for 1 challenge ?
```

## Additional Benefits

1. **Better UX**: Visual feedback - button is visually disabled while processing
2. **Prevents accidental double-clicks**: Even non-cheaters benefit
3. **Server-friendly**: If we add online leaderboards later, prevents spam submissions
4. **Race condition protection**: Ensures answer evaluation completes before next submission

## Testing Checklist

- [ ] Single click works normally
- [ ] Rapid clicking only counts first click
- [ ] Button re-enables on new challenge
- [ ] Button stays disabled during 1.5s auto-advance
- [ ] Works for both ClockToTime and TimeToClock modes
- [ ] Game over state still works (button hidden)

## Related Code

- **ViewModel**: `ClockExerciser/ViewModels/GameViewModel.cs`
- **UI Binding**: `ClockExerciser/GamePage.xaml` - `IsEnabled="{Binding CanSubmit}"`
- **Command**: `CheckAnswerCommand`
