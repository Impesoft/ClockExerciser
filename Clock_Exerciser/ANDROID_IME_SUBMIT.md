# Android IME Action & Keyboard Dismissal

## Overview
Configured the answer input field to:
1. Trigger the submit/check button when pressing Enter/Go
2. **Dismiss the keyboard automatically** after submission

---

## What Was Changed

### 1. Keyboard Helper JavaScript (`keyboardHelper.js`) - NEW

**Created utility module** for keyboard operations:
```javascript
export function dismissKeyboard() {
	if (document.activeElement && document.activeElement instanceof HTMLInputElement) {
		document.activeElement.blur();
	}
}
```

This JavaScript function:
- Checks if an input is currently focused
- Blurs (unfocuses) it, which dismisses the mobile keyboard

### 2. AnswerInput Component (`AnswerInput.razor`)

**Added**:
```html
<input @ref="inputElement"          <!-- NEW: Reference for JS interop -->
	   class="answer-input"
	   type="text"
	   inputmode="text"
	   enterkeyhint="go"
	   placeholder="@Placeholder"
	   value="@Value"
	   @oninput="OnInput"
	   @onkeydown="OnKeyDown" />
```

**Attributes**:
- `@ref="inputElement"` → Element reference for programmatic access
- `enterkeyhint="go"` → Android IME shows "Go" button (✓)
- `@onkeydown="OnKeyDown"` → Handles Enter key press

### 3. AnswerInput Code-Behind (`AnswerInput.razor.cs`)

**Added**:
- `DismissKeyboardAsync()` method - calls JavaScript to blur input
- `IAsyncDisposable` implementation - cleans up JS module
- Keyboard dismissal in `OnKeyDown()` when Enter is pressed

```csharp
public async Task DismissKeyboardAsync()
{
	try
	{
		_keyboardModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./keyboardHelper.js");
		await _keyboardModule.InvokeVoidAsync("dismissKeyboard");
	}
	catch
	{
		// Silently fail - keyboard dismissal is not critical
	}
}

private async Task OnKeyDown(KeyboardEventArgs args)
{
	if (args.Key == "Enter" && OnEnterKeyPressed.HasDelegate)
	{
		await DismissKeyboardAsync();  // NEW: Dismiss before submitting
		await OnEnterKeyPressed.InvokeAsync();
	}
}
```

### 4. Game Page (`Game.razor`)

**Added component reference**:
```razor
<AnswerInput @ref="answerInput"     <!-- NEW: Reference to component -->
			 Placeholder="@State.Text(AppTextKeys.EntryPlaceholder)"
			 Value="@State.AnswerText"
			 ValueChanged="OnAnswerInput"
			 OnEnterKeyPressed="OnPrimaryActionAsync" />
```

### 5. Game Code-Behind (`Game.razor.cs`)

**Updated submit handler**:
```csharp
private async Task OnPrimaryActionAsync()
{
	// Dismiss keyboard when submitting answer
	if (answerInput is not null)
	{
		await answerInput.DismissKeyboardAsync();  // NEW
	}

	await State.ExecutePrimaryActionAsync();
}
```

---

## How It Works

### When User Presses "Go" on Android Keyboard
1. `@onkeydown` event fires
2. `OnKeyDown()` detects `Key == "Enter"`
3. **`DismissKeyboardAsync()` is called** → keyboard closes
4. `OnEnterKeyPressed` callback fires
5. `OnPrimaryActionAsync()` executes
6. Answer is submitted and validated
7. ✅ User can see the result message (no keyboard blocking it)

### When User Clicks the Check Button
1. User taps the "Check" button
2. `OnPrimaryActionAsync()` is called
3. **`answerInput.DismissKeyboardAsync()` is called** → keyboard closes
4. `State.ExecutePrimaryActionAsync()` executes
5. Answer is validated
6. ✅ Result message is visible (keyboard dismissed)

### Web Browser Behavior
Same flow works for desktop/mobile browsers:
- Enter key → keyboard dismissed → answer submitted
- Button click → keyboard dismissed → answer submitted

---

## User Experience Flow

```
┌─────────────────────────────────────┐
│ User types answer: "10:30"          │
└─────────────────────────────────────┘
			  │
			  ▼
┌─────────────────────────────────────┐
│ Presses "Go" button OR Check button │
└─────────────────────────────────────┘
			  │
			  ▼
┌─────────────────────────────────────┐
│ ⌨️ KEYBOARD DISMISSES (blur input)  │
└─────────────────────────────────────┘
			  │
			  ▼
┌─────────────────────────────────────┐
│ Answer validated                    │
└─────────────────────────────────────┘
			  │
			  ▼
┌─────────────────────────────────────┐
│ ✅ Result message visible           │
│ (no keyboard blocking it!)          │
└─────────────────────────────────────┘
```

---

## Android IME Actions Reference

| Action | Purpose | Button Label | Keyboard Behavior |
|--------|---------|--------------|-------------------|
| `IME_ACTION_NEXT` | Move to next field | → Next | Stays open |
| `IME_ACTION_GO` | **Submit/Confirm** | ✓ Go | **Dismissed** |
| `IME_ACTION_DONE` | Dismiss keyboard | ✓ Done | Dismissed |
| `IME_ACTION_SEARCH` | Search action | 🔍 Search | Stays open |
| `IME_ACTION_SEND` | Send message | ✈ Send | Dismissed |

**We use**: `enterkeyhint="go"` (HTML5 equivalent of `IME_ACTION_GO`)

---

## Testing

### Android
1. Run the app on Android device/emulator
2. Start a "Clock to Time" game
3. Tap the input field → keyboard appears
4. Type an answer (e.g., "10:30")
5. **Press "Go" button on keyboard**
6. ✅ Keyboard should **dismiss immediately**
7. ✅ Result message should be **fully visible**

**Alternative**: Tap the "Check" button instead of "Go"
- ✅ Same result: keyboard dismisses, answer submitted

### Web Browser (Desktop)
1. Open in browser
2. Start a "Clock to Time" game
3. Type an answer
4. Press Enter or click Check
5. ✅ Input loses focus (keyboard would dismiss on mobile)

### iOS
1. iOS keyboards typically show "Go" or "Done"
2. Same keyboard dismissal behavior
3. ✅ Keyboard closes after submission

---

## Advantages

✅ **Better UX**: Keyboard doesn't block result message  
✅ **Faster workflow**: Keyboard dismisses automatically  
✅ **Native feel**: Uses platform keyboard blur behavior  
✅ **Dual trigger**: Works with keyboard "Go" AND check button  
✅ **Cross-platform**: Works on Android, iOS, and web  

---

## Technical Details

### JavaScript Interop
- Uses ES6 module import: `import("./keyboardHelper.js")`
- Module is loaded lazily (only when needed)
- Disposed properly via `IAsyncDisposable`
- Errors are caught and ignored (non-critical operation)

### Why Blur Instead of Hide?
- Blurring (unfocusing) the input is the **standard web API**
- Mobile browsers automatically hide the keyboard when an input loses focus
- No platform-specific code needed
- Works consistently across Android, iOS, and web

### Performance
- JavaScript module is cached after first load
- Blur operation is instantaneous
- No noticeable delay in submission

---

## Code Summary

**Files Created**:
- `keyboardHelper.js` - JavaScript keyboard utilities

**Files Modified**:
- `AnswerInput.razor` - Added `@ref` for element reference
- `AnswerInput.razor.cs` - Added `DismissKeyboardAsync()` method
- `Game.razor` - Added `@ref="answerInput"` 
- `Game.razor.cs` - Calls `DismissKeyboardAsync()` on submit

**Build Status**: ✅ Successful

---

**Date**: 2026-05-15  
**Features**: 
- Android IME Go Action → Submit Answer
- Automatic Keyboard Dismissal on Submit

