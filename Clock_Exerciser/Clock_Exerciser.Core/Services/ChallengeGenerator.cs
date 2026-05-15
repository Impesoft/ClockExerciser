using Clock_Exerciser.Core.Models;

namespace Clock_Exerciser.Core.Services;

public sealed class ChallengeGenerator
{
    private readonly Random _random = new();

    public GameChallenge Create(GameMode requestedMode)
    {
        var mode = requestedMode == GameMode.Random
            ? (_random.Next(0, 2) == 0 ? GameMode.ClockToTime : GameMode.TimeToClock)
            : requestedMode;

        return new GameChallenge(mode, CreateRandomTime());
    }

    private TimeSpan CreateRandomTime()
    {
        var hour = _random.Next(0, 24);
        var minute = _random.Next(0, 12) * 5;
        return new TimeSpan(hour, minute, 0);
    }
}
