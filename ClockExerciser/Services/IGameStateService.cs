using ClockExerciser.Models;

namespace ClockExerciser.Services;

/// <summary>
/// Service for managing game state across navigation and screen changes
/// </summary>
public interface IGameStateService
{
    /// <summary>
    /// Number of correct answers in the current game session
    /// </summary>
    int CorrectAnswers { get; set; }
    
    /// <summary>
    /// Number of wrong answers in the current game session
    /// </summary>
    int WrongAnswers { get; set; }
    
    /// <summary>
    /// Current active game mode
    /// </summary>
    GameMode ActiveMode { get; set; }
    
    /// <summary>
    /// High score across all game sessions
    /// </summary>
    int HighScore { get; set; }
    
    /// <summary>
    /// Maximum wrong answers before game over
    /// </summary>
    int MaxWrongAnswers { get; }
    
    /// <summary>
    /// Indicates whether the game is over
    /// </summary>
    bool IsGameOver { get; }
    
    /// <summary>
    /// Resets the game state for a new game
    /// </summary>
    void ResetGame();
    
    /// <summary>
    /// Occurs when game state changes
    /// </summary>
    event EventHandler? GameStateChanged;
}
