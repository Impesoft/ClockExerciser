namespace ClockExerciser.Models;

/// <summary>
/// Option for difficulty level selection
/// </summary>
/// <param name="DisplayName">Display name for the difficulty level</param>
/// <param name="Level">The difficulty level enum value</param>
public record DifficultyOption(string DisplayName, DifficultyLevel Level);
