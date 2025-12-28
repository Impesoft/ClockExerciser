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
    /// Current difficulty level
    /// </summary>
    DifficultyLevel CurrentDifficulty { get; set; }
    
    /// <summary>
    /// High score across all game sessions
    /// </summary>
    int HighScore { get; set; }
    
    /// <summary>
    /// Maximum wrong answers before game over (based on difficulty)
    /// </summary>
    int MaxWrongAnswers { get; }
    
    /// <summary>
    /// Indicates whether the game is over (only applicable for Normal/Advanced)
    /// </summary>
    bool IsGameOver { get; }
    
    /// <summary>
    /// Calculates effective score (correct answers minus wrong answers for Beginner)
    /// </summary>
    int EffectiveScore { get; }
    
    /// <summary>
    /// Resets the game state for a new game
    /// </summary>
    void ResetGame();
    
    /// <summary>
    /// Occurs when game state changes
    /// </summary>
    event EventHandler? GameStateChanged;
}
