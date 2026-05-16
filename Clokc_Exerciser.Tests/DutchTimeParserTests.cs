using Clock_Exerciser.Core.Services;

namespace Clock_Exerciser.Tests;

public class DutchTimeParserTests
{
    private readonly DutchTimeParser _parser = new();

    [Fact]
    public void Parse_5VoorHalf11_Returns1025()
    {
        var result = _parser.Parse("5 voor half 11");
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(10, 25, 0), result.Value);
    }

    [Fact]
    public void Parse_5VoorHalf8_Returns725()
    {
        var result = _parser.Parse("5 voor half 8");
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(7, 25, 0), result.Value);
    }

    [Fact]
    public void Parse_5NaHalf10_Returns935()
    {
        var result = _parser.Parse("5 na half 10");
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(9, 35, 0), result.Value);
    }

    [Fact]
    public void Parse_10VoorHalf3_Returns220()
    {
        var result = _parser.Parse("10 voor half 3");
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(2, 20, 0), result.Value);
    }
}
