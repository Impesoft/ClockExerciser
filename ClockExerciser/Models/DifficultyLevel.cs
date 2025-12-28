namespace ClockExerciser.Models;

/// <summary>
/// Difficulty levels for the game
/// </summary>
public enum DifficultyLevel
{
    /// <summary>
    /// Beginner: Unlimited tries, wrong answers subtract from score
    /// </summary>
    Beginner,
    
    /// <summary>
    /// Normal: 5 wrong answers allowed before game over
    /// </summary>
    Normal,
    
    /// <summary>
    /// Advanced: 3 wrong answers allowed before game over
    /// </summary>
    Advanced
}
