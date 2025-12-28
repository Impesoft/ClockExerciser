using ClockExerciser.Services;

namespace ClockExerciser.Tests;

/// <summary>
/// Manual test cases for English time parser improvements
/// Run these in debug mode to verify parser handles the reported cases
/// </summary>
public class EnglishTimeParserTests
{
    private readonly EnglishTimeParser _parser = new();

    public void TestReportedCases()
    {
        // Test case 1: "Five before 12" should parse to 11:55
        var result1 = _parser.Parse("Five before 12");
        System.Diagnostics.Debug.Assert(result1 == new TimeSpan(11, 55, 0), 
            $"'Five before 12' should be 11:55, got {result1}");

        // Test case 2: "5 to 12" should parse to 11:55
        var result2 = _parser.Parse("5 to 12");
        System.Diagnostics.Debug.Assert(result2 == new TimeSpan(11, 55, 0), 
            $"'5 to 12' should be 11:55, got {result2}");

        // Test case 3: "25 past 3" should parse to 3:25
        var result3 = _parser.Parse("25 past 3");
        System.Diagnostics.Debug.Assert(result3 == new TimeSpan(3, 25, 0), 
            $"'25 past 3' should be 3:25, got {result3}");

        // Additional test: "five of 12" (alternative phrasing)
        var result4 = _parser.Parse("five of 12");
        System.Diagnostics.Debug.Assert(result4 == new TimeSpan(11, 55, 0), 
            $"'five of 12' should be 11:55, got {result4}");

        System.Diagnostics.Debug.WriteLine("? All parser tests passed!");
    }
}
