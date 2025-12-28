using ClockExerciser.Models;
using Microsoft.Maui.Storage;

namespace ClockExerciser.Services;

/// <summary>
/// Service for managing game state across navigation and screen changes.
/// Persists score, wrong answers, and game mode so users can switch screens without losing progress.
/// </summary>
public sealed class GameStateService : IGameStateService
{
    private int _correctAnswers;
    private int _wrongAnswers;
    private GameMode _activeMode = GameMode.ClockToTime;
    private DifficultyLevel _currentDifficulty = DifficultyLevel.Normal;

    public event EventHandler? GameStateChanged;

    public int CorrectAnswers
    {
        get => _correctAnswers;
        set
        {
            if (_correctAnswers != value)
            {
                _correctAnswers = value;
                
                // Update high score if current effective score is higher
                if (EffectiveScore > HighScore)
                {
                    HighScore = EffectiveScore;
                }
                
                OnGameStateChanged();
            }
        }
    }

    public int WrongAnswers
    {
        get => _wrongAnswers;
        set
        {
            if (_wrongAnswers != value)
            {
                _wrongAnswers = value;
                OnGameStateChanged();
            }
        }
    }

    public GameMode ActiveMode
    {
        get => _activeMode;
        set
        {
            if (_activeMode != value)
            {
                _activeMode = value;
                OnGameStateChanged();
            }
        }
    }

    public DifficultyLevel CurrentDifficulty
    {
        get => _currentDifficulty;
        set
        {
            if (_currentDifficulty != value)
            {
                _currentDifficulty = value;
                OnGameStateChanged();
            }
        }
    }

    public int HighScore
    {
        get => Preferences.Get("HighScore", 0);
        set
        {
            Preferences.Set("HighScore", value);
            OnGameStateChanged();
        }
    }

    public int MaxWrongAnswers => CurrentDifficulty switch
    {
        DifficultyLevel.Beginner => int.MaxValue, // Unlimited
        DifficultyLevel.Normal => 5,
        DifficultyLevel.Advanced => 3,
        _ => 3
    };

    public bool IsGameOver => CurrentDifficulty != DifficultyLevel.Beginner && _wrongAnswers >= MaxWrongAnswers;

    public int EffectiveScore => CurrentDifficulty == DifficultyLevel.Beginner 
        ? Math.Max(0, _correctAnswers - _wrongAnswers) // Subtract wrong answers for Beginner
        : _correctAnswers; // Normal and Advanced don't penalize in score

    public void ResetGame()
    {
        CorrectAnswers = 0;
        WrongAnswers = 0;
        OnGameStateChanged();
    }

    private void OnGameStateChanged()
    {
        GameStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
