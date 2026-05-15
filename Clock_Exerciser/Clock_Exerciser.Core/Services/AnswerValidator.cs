namespace Clock_Exerciser.Core.Services;

public sealed class AnswerValidator
{
    public bool EvaluateTextAnswer(TimeSpan userTime, TimeSpan targetTime) => ClockMath.MatchesTime(userTime, targetTime);

    public bool EvaluateClockAnswer(double userHourValue, double userMinuteValue, TimeSpan targetTime)
    {
        var userHour = (int)userHourValue;
        var targetHour = targetTime.Hours % 12;
        if (targetHour == 0)
        {
            targetHour = 12;
        }

        var userMinute = (int)userMinuteValue;
        var targetMinute = targetTime.Minutes;

        var hoursMatch = userHour == targetHour
            || (userHour == 0 && targetHour == 12)
            || (userHour == 12 && targetHour == 0);

        var minuteDiff = Math.Abs(userMinute - targetMinute);
        var minutesMatch = minuteDiff <= 1;

        return hoursMatch && minutesMatch;
    }
}
