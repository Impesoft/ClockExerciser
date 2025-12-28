using ClockExerciser.Models;
using Microsoft.Maui.Storage;

namespace ClockExerciser.Services;

/// <summary>
/// Service for managing game state across navigation and screen changes.
/// Persists score, wrong answers, and game mode so users can switch screens without losing progress.
/// </summary>
public sealed class GameStateService : IGameStateService
{
    private const int DefaultMaxWrongAnswers = 3;
    private int _correctAnswers;
    private int _wrongAnswers;
    private GameMode _activeMode = GameMode.ClockToTime;

    public event EventHandler? GameStateChanged;

    public int CorrectAnswers
    {
        get => _correctAnswers;
        set
        {
            if (_correctAnswers != value)
            {
                _correctAnswers = value;
                
                // Update high score if current score is higher
                if (_correctAnswers > HighScore)
                {
                    HighScore = _correctAnswers;
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

    public int HighScore
    {
        get => Preferences.Get("HighScore", 0);
        set
        {
            Preferences.Set("HighScore", value);
            OnGameStateChanged();
        }
    }

    public int MaxWrongAnswers => DefaultMaxWrongAnswers;

    public bool IsGameOver => _wrongAnswers >= MaxWrongAnswers;

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
