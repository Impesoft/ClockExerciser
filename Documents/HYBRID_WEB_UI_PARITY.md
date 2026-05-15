# Clock Exerciser Hybrid/Web UI Parity Targets

This document captures the legacy XAML layout that the new Blazor Hybrid/Web implementation must recreate using `.razor`, `.razor.cs`, and `.razor.css` files. The new UI should preserve layout and behavior as closely as practical while replacing Syncfusion gauges with shared SVG/CSS clock components.

## Source Screens

- Legacy menu: `ClockExerciser/MenuPage.xaml`
- Legacy game: `ClockExerciser/GamePage.xaml`
- Behavior references: `ClockExerciser/ViewModels/MenuViewModel.cs`, `ClockExerciser/ViewModels/GameViewModel.cs`

## Global Layout Rules

- Use a vertically scrolling page to support small mobile screens.
- Use centered content with `24px` page padding where the XAML uses `Padding="24"`.
- Preserve the simple educational visual hierarchy: large title, clock visual, primary action buttons, language selector, then gameplay controls.
- Preserve light/dark theme color intent where possible:
  - Menu clock light background: `#e3f2fd`
  - Menu clock light stroke: `#2196f3`
  - Game clock light background: `#f5f5f5`
  - Game dark clock background: `#1f1f1f`
  - Primary blue: `#2196f3`
  - Success green: `#4caf50` / result `#2e7d32`
  - Random orange: `#ff9800`
  - Error red: `#c62828`
  - Second hand red/orange: `#d84315`

## Menu Page Target

### Route

- Razor route: `/`
- Equivalent legacy page: `MenuPage.xaml`

### Structure

1. Scrollable root.
2. Vertical stack with:
   - padding `24px`
   - spacing around `32px`
   - centered content.
3. Title block:
   - `MenuTitle`
   - font size equivalent to `36`
   - bold
   - centered
   - `MenuSubtitle`
   - font size equivalent to `18`
   - centered
   - opacity around `0.7`
4. Circular clock preview:
   - 180x180
   - centered
   - circular border
   - light/dark themed background and stroke
   - displays 3:15 with red second hand at 12
   - uses SVG/CSS in new implementation, not Syncfusion.
5. Mode button stack:
   - vertical spacing `16px`
   - three full-width buttons
   - height around `60px`
   - font size around `18px`
   - border radius around `12px`
   - colors:
	 - Clock to Time: blue
	 - Time to Clock: green
	 - Random: orange
6. Language selection:
   - vertical stack spacing around `8px`
   - bold centered label
   - native language names (`English`, `Nederlands`)
   - selection width around `200px`
   - centered.

### Menu Bindings/Behavior

- `MenuTitle` -> localized title.
- `MenuSubtitle` -> localized subtitle.
- Mode buttons navigate to game route with mode parameter.
- Language selector changes culture and updates all visible text.
- No navigation bar chrome in the Hybrid page content.

## Game Page Target

### Route

- Razor route: `/game/{mode}` or `/game?mode=...`
- Equivalent legacy page: `GamePage.xaml`

### Structure

1. Scrollable root.
2. Vertical stack with:
   - padding `24px`
   - spacing around `18px`.
3. Instruction label:
   - `ClockToTimeInstruction` when mode is ClockToTime.
   - `TimeToClockInstruction` when mode is TimeToClock.
4. Main analog clock:
   - centered
   - around 320x320
   - circular border/background
   - one 12-hour face
   - labels show 12 instead of 0
   - minor minute divisions equivalent to the old gauge
   - three hands:
	 - hour hand: length around 60%, dark, width around 5
	 - minute hand: length around 90%, dark, width around 4
	 - second hand: length around 100%, red/orange, thin
   - no Syncfusion; use SVG/CSS and shared clock math.
5. Time-to-clock prompt section, visible only in TimeToClock mode:
   - bold `PromptLabel`
   - friendly text (`PromptText`)
   - bold digital text (`PromptDigital`)
6. Clock-to-time input, visible only in ClockToTime mode:
   - text input with `EntryPlaceholder`
   - two-way answer text binding.
7. Time-to-clock sliders, visible only in TimeToClock mode:
   - bold `HourLabel`
   - hour slider min 0 max 12
   - bold `MinuteLabel`
   - minute slider min 0 max 59
   - no visible second slider.
8. Score display:
   - horizontal area aligned to end
   - label `Score:` currently hardcoded in legacy page
   - score text large, bold, green.
9. Primary action button:
   - full width
   - text is `SubmitAnswer` or `NextChallenge` after a correct answer
   - disabled in ClockToTime mode when answer text is blank
10. Result label:
   - centered
   - bold
   - visible only when result is available
   - green on success
   - red on incorrect.

### Game Behavior

- Generate a new random challenge when entering a mode.
- Random mode chooses between ClockToTime and TimeToClock per challenge.
- ClockToTime displays target hour/minute and ticking visual second hand.
- TimeToClock displays prompt text and user-controlled hour/minute hands, plus ticking visual second hand.
- Submit validates answer.
- Correct answer increments persisted score.
- Correct result changes primary button to next challenge.
- Incorrect answer keeps user on current challenge.
- Success/error audio is attempted through host-specific audio adapter.
- Language changes update all labels and prompt text.
- Second-hand timer must stop when the component/page is disposed to avoid disposed-view updates in Hybrid.

## Razor File Requirements

- Page/component UI must be split into:
  - `.razor` markup
  - `.razor.cs` code-behind
  - `.razor.css` scoped styles
- Keep markup close to the legacy page section order.
- Use components only where they help preserve clarity and reuse without changing the visual order.

## Components to Create

- `AnalogClock.razor` / `.razor.cs` / `.razor.css`
- `LanguageSelector.razor` / `.razor.cs` / `.razor.css`
- `ModeButton.razor` / `.razor.cs` / `.razor.css`
- `ScoreDisplay.razor` / `.razor.cs` / `.razor.css`
- `ResultMessage.razor` / `.razor.cs` / `.razor.css`
- optional gameplay input components if the game page becomes too large.

## Acceptance Checklist

- Menu page visually follows the legacy order and proportions.
- Game page visually follows the legacy order and proportions.
- Clock face uses one 12-hour dial with correctly scaled minute/second hands.
- No Syncfusion references exist in the new projects.
- No new XAML UI is added beyond the MAUI Hybrid host page required for `BlazorWebView`.
- Hybrid and Web render the same shared Razor pages.
- English/Dutch switching works from both menu and game pages.
- Natural language input works for English and Dutch.
- Score persists per host.
- Timer cleanup prevents disposed view updates in Hybrid.
