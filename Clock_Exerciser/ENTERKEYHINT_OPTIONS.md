# Quick Reference: enterkeyhint Options

Choose the button label you want:

## Option 1: "Done" Button (Current)
```html
enterkeyhint="done"
```
- ✅ Shows "Done" or "✓" on keyboard
- ✅ Signals completion
- ✅ Android more likely to auto-dismiss
- Use for: Final input in a form

## Option 2: "Go" Button (Alternative)
```html
enterkeyhint="go"
```
- ✅ Shows "Go" or "→" on keyboard  
- ✅ Signals submit/action
- ⚠️ Android may keep keyboard open (we handle dismissal in JS anyway)
- Use for: Search, navigation, actions

## Current Setting
**File**: `Clock_Exerciser\Clock_Exerciser.Shared\Components\AnswerInput.razor`  
**Line 5**: `enterkeyhint="done"`

## To Change to "Go"
Replace line 5 with:
```html
enterkeyhint="go"
```

**Either way**, our multi-strategy JavaScript dismissal will close the keyboard! The difference is only the button label.

## Recommendation
Keep **"done"** because:
1. User is finishing their answer (not navigating)
2. Android better understands dismissal intent
3. Better semantic meaning for this use case

But **"go"** works fine too if you prefer that label! 🚀
