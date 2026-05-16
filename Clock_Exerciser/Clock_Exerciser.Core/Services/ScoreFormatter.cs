namespace Clock_Exerciser.Core.Services;

public static class ScoreFormatter
{
    public static string Format(int correctAnswers, int errorCount) => $"🏆 {correctAnswers}/{errorCount}";
}
