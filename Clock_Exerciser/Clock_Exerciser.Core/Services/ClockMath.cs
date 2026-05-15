namespace Clock_Exerciser.Core.Services;

public static class ClockMath
{
    public static double ConvertToDialValue(double rawValue) => rawValue / 5d;

    public static double GetHourPointerValue(TimeSpan time)
    {
        var hours = time.Hours % 12;
        return hours + time.Minutes / 60d + time.Seconds / 3600d;
    }

    public static double GetMinutePointerValue(TimeSpan time) => ConvertToDialValue(time.Minutes + time.Seconds / 60d);

    public static double GetSecondPointerValue(int second) => ConvertToDialValue(second);

    public static bool MatchesTime(TimeSpan candidate, TimeSpan target)
    {
        var candidateHours = (candidate.Hours % 12) + 12;
        var targetHours = (target.Hours % 12) + 12;
        var hoursMatch = candidateHours == targetHours;
        var minuteDiff = Math.Abs(candidate.Minutes - target.Minutes);
        var minutesMatch = minuteDiff <= 1;

        return hoursMatch && minutesMatch;
    }

    public static double CircularDifference(double a, double b, double period)
    {
        var diff = Math.Abs(a - b);
        return Math.Min(diff, period - diff);
    }
}
