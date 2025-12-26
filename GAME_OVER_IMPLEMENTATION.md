# Game Over & High Score Implementation

## Overview
Added a "3 strikes and you're out" game mechanic with high score tracking to make the Clock Exerciser more engaging and give the score system real purpose.

## Changes Made

### 1. **GameViewModel.cs** - Core Game Logic

#### New Fields
- `_wrongAnswers` - Tracks incorrect answers in current game
- `MaxWrongAnswers = 3` - Constant for game over threshold

#### New Properties
- `WrongAnswers` - Current wrong answer count
- `HighScore` - Persisted high score (saved to Preferences)
- `IsGameOver` - Boolean indicating if game is over (3 wrong answers)
- `ScoreText` - Formatted score display with trophy emoji
- `HighScoreText` - Formatted high score with star emoji
- `WrongAnswersText` - Formatted wrong answers (e.g., "? 2/3")
- `HighScoreLabel`, `WrongAnswersLabel`, `NewGameButtonText` - Localized labels

#### New Commands
- `NewGameCommand` - Command to start a new game

#### Game Flow Changes
1. **ExecutePrimaryAction()** - Now checks if game is over before allowing answer submission
2. **ExecuteCheckAnswer()** - Still auto-advances on correct answers, but doesn't advance on game over
3. **StartNewGame()** - Resets all game state (score, wrong answers, UI) and generates first challenge
4. **GenerateNewChallenge()** - Now checks IsGameOver to prevent generating challenges after game ends
5. **SetResult()** - Updates scores, increments wrong answers on failure, checks for game over, updates high score

#### High Score Logic
- High score is updated whenever the current score exceeds it
- High score persists across app sessions using `Preferences`
- Displayed prominently in the UI

### 2. **GamePage.xaml** - UI Updates

#### Score Display
Changed from simple score to comprehensive game stats:
```xml
<Grid ColumnDefinitions="Auto,*,Auto">
    <!-- Current Score (left) -->
    <!-- Wrong Answers (center) -->
    <!-- High Score (right) -->
</Grid>
```

#### Button Logic
- **Check Answer Button**: Hidden when game is over (using InvertedBoolConverter)
- **New Game Button**: Shown only when game is over with distinct styling

### 3. **InvertedBoolConverter.cs** - New Converter
Created a new value converter to invert boolean values for UI visibility logic.

### 4. **Resource Strings** - Localization

#### English (AppResources.en.resx)
- `GameOver`: "Game Over! Your score: {0}. Try again?"
- `NewGame`: "New Game"
- `HighScoreLabel`: "High Score:"
- `WrongAnswersLabel`: "Wrong Answers:"

#### Dutch (AppResources.nl-NL.resx)
- `GameOver`: "Game Over! Jouw score: {0}. Opnieuw proberen?"
- `NewGame`: "Nieuw Spel"
- `HighScoreLabel`: "Hoogste Score:"
- `WrongAnswersLabel`: "Foute Antwoorden:"

## Game Mechanics

### Starting a Game
- Navigate to game mode from menu
- `ApplyQueryAttributes` ? `StartNewGame()`
- Score: 0, Wrong Answers: 0/3
- First challenge generated

### Playing
- **Correct Answer**:
  - Score increments (?? +1)
  - Success message shows for 1.5 seconds
  - Auto-advances to next challenge
  - High score updates if exceeded
  
- **Wrong Answer**:
  - Wrong answer count increments (? +1)
  - Error message shows
  - User can try again or continue to next challenge manually
  - After 3rd wrong answer ? Game Over

### Game Over
- "Game Over! Your score: X. Try again?" message
- Check Answer button hidden
- New Game button appears
- User can review final stats before restarting

### High Score
- Persists across app restarts
- Updates immediately when current score exceeds it
- Always visible during gameplay for motivation

## User Experience Improvements

1. **Clear Goal**: Get the highest score possible before 3 mistakes
2. **Progress Tracking**: See wrong answers accumulate (? 1/3, 2/3, 3/3)
3. **Competition**: Beat your own high score
4. **Forgiveness**: 3 chances to make mistakes
5. **Quick Restart**: One tap to start new game
6. **Visual Feedback**: Color-coded stats (green=score, red=wrong, orange=high score)

## Technical Notes

- High score stored in `Preferences` as "HighScore"
- Game state resets properly when starting new game
- Language switching updates all new labels
- Auto-advance still works for correct answers
- Game over state prevents further gameplay until restart

## Future Enhancements (Optional)

- Different difficulty levels (e.g., 5 strikes for easy, 2 for hard)
- Global leaderboard
- Statistics tracking (total games played, accuracy %)
- Achievement system
- Daily challenge mode
