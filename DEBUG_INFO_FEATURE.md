# Debug Information Feature

## Overview
Added debug information to show parsed time values when answers are incorrect during DEBUG builds.

## What Was Added

### For "Clock to Time" Mode (Text Entry)
When you enter an incorrect answer, the error message will show:
```
Close, try again. (You: 03:15:00, Target: 03:30:00)
```

This helps you see:
- What time your input was parsed as
- What the actual target time was
- Why the answer was marked incorrect

### For "Time to Clock" Mode (Clock Hands)
When you set the clock hands incorrectly, the error message will show:
```
Close, try again. (You: 03:20:00, Target: 03:30:00)
```

This shows:
- What time your clock hands represented
- What the target time was
- How far off you were

## Implementation Details

### Debug Build Only
The debug information is **only shown in DEBUG builds**:
```csharp
#if DEBUG
ResultMessage = baseMessage + (debugInfo ?? string.Empty);
#else
ResultMessage = baseMessage;
#endif
```

### Release Build
In RELEASE builds (production), users will only see:
- "Correct! Try another."
- "Close, try again."

No debug information is shown to end users.

## Benefits for Development

### 1. Testing Time Parsers
Quickly verify that time parsing works correctly:
- Enter "quarter past three" ? See if it parsed as 03:15:00
- Enter "half vijf" (Dutch) ? See if it parsed as 04:30:00
- Enter "3:15" ? See if it parsed as 03:15:00

### 2. Testing Clock Hand Validation
See exactly what time the sliders represent:
- Set hour=3, minute=15, second=0
- Check if it shows as 03:15:00
- Verify the circular difference calculation

### 3. Debugging Tolerance Issues
Understand why an answer might be marked incorrect:
- See the exact difference between user input and target
- Verify the 1-minute tolerance is working
- Check 12/24 hour equivalence

### 4. Language Testing
Test natural language parsing in both languages:
- English: "ten to four" ? Should show 03:50:00
- Dutch: "tien voor vier" ? Should show 03:50:00

## Usage During Testing

### Test Scenario 1: Valid Digital Time
1. Target time: 03:30
2. Enter: "3:30"
3. Expected: ? Success (no debug info shown)

### Test Scenario 2: Wrong Time
1. Target time: 03:30
2. Enter: "3:15"
3. Expected: ? "Close, try again. (You: 03:15:00, Target: 03:30:00)"

### Test Scenario 3: Natural Language
1. Target time: 03:15
2. Enter: "quarter past three"
3. Expected: ? Success

### Test Scenario 4: Natural Language Wrong
1. Target time: 03:30
2. Enter: "quarter past three"
3. Expected: ? "Close, try again. (You: 03:15:00, Target: 03:30:00)"

### Test Scenario 5: Dutch Natural Language
1. Target time: 04:30
2. Switch to Nederlands
3. Enter: "half vijf"
4. Expected: ? Success

### Test Scenario 6: Clock Hands
1. Target time: 03:30
2. Set hour slider to 3, minute slider to 20
3. Expected: ? "Close, try again. (You: 03:20:00, Target: 03:30:00)"

## Format Details

### Time Display Format
- Format: `hh:mm:ss` (hours:minutes:seconds)
- Hours: Always 2 digits (03 not 3)
- Minutes: Always 2 digits
- Seconds: Always 2 digits
- Example: 03:15:00, 11:45:30, 12:00:00

### Debug String Format
```
(You: HH:MM:SS, Target: HH:MM:SS)
```

## Code Location

**File**: `ClockExerciser/ViewModels/GameViewModel.cs`

**Methods Modified**:
- `EvaluateTextAnswer()` - Added debug info for text entry mode
- `EvaluateClockAnswer()` - Added debug info for clock hand mode
- `SetResult(bool success, string? debugInfo = null)` - Updated to accept optional debug info

## Testing Checklist

Use this debug feature to verify:

- [ ] Digital time parsing (3:15, 03:15, 15:30)
- [ ] English natural language (quarter past, half past, ten to)
- [ ] Dutch natural language (kwart over, half, voor)
- [ ] Clock hand positioning (hour, minute, second sliders)
- [ ] 12/24 hour equivalence (3:15 PM = 15:15)
- [ ] Tolerance levels (1 minute for time entry)
- [ ] Edge cases (midnight, noon)
- [ ] Language switching (English ? Dutch)

## Removing Debug Info for Production

When ready for production release:
1. Build in **Release** mode
2. Debug info automatically excluded via `#if DEBUG` directive
3. No code changes needed

Or to remove completely:
1. Remove debug info generation in `EvaluateTextAnswer()` and `EvaluateClockAnswer()`
2. Simplify `SetResult()` method to not accept debugInfo parameter

## Example Debug Session

```
Test: Dutch "half vijf" parsing
Target: 04:30:00
Enter: "half vijf"
Result: ? Correct! Try another.

Test: Dutch "half vijf" with wrong target
Target: 05:30:00
Enter: "half vijf"  
Result: ? Close, try again. (You: 04:30:00, Target: 05:30:00)
Analysis: Parser correctly interpreted "half vijf" as 4:30
```

## Known Limitations

1. Only shows in DEBUG builds (intended behavior)
2. Shows seconds even though game doesn't require exact seconds
3. Time shown is the parsed value, not the raw input string
4. For clock hands, shows the integer values (not fractional movement)

## Future Enhancements

Could add:
- Show raw input string as well
- Show which parser was used (digital/English/Dutch)
- Show the tolerance used for matching
- Show the differences for each component (hour/minute/second)

---

**Status**: ? Implemented  
**Build**: Requires rebuild in Visual Studio  
**Testing**: Ready to use for debugging  
**Production**: Auto-disabled in Release builds
