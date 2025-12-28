using ClockExerciser.Services;

namespace ClockExerciser.Tests;

/// <summary>
/// Manual test cases for Dutch time parser improvements
/// Run these in debug mode to verify parser handles various Dutch time expressions
/// </summary>
public class DutchTimeParserTests
{
    private readonly DutchTimeParser _parser = new();

    public void TestDutchTimeExpressions()
    {
        // Test case 1: "tien over drie" should parse to 3:10
        var result1 = _parser.Parse("tien over drie");
        System.Diagnostics.Debug.Assert(result1 == new TimeSpan(3, 10, 0), 
            $"'tien over drie' should be 3:10, got {result1}");

        // Test case 2: "tien na drie" (alternative to "over") should parse to 3:10
        var result2 = _parser.Parse("tien na drie");
        System.Diagnostics.Debug.Assert(result2 == new TimeSpan(3, 10, 0), 
            $"'tien na drie' should be 3:10, got {result2}");

        // Test case 3: "10 over 3" (numeric) should parse to 3:10
        var result3 = _parser.Parse("10 over 3");
        System.Diagnostics.Debug.Assert(result3 == new TimeSpan(3, 10, 0), 
            $"'10 over 3' should be 3:10, got {result3}");

        // Test case 4: "vijf voor twaalf" should parse to 11:55
        var result4 = _parser.Parse("vijf voor twaalf");
        System.Diagnostics.Debug.Assert(result4 == new TimeSpan(11, 55, 0), 
            $"'vijf voor twaalf' should be 11:55, got {result4}");

        // Test case 5: "kwart over vijf" should parse to 5:15
        var result5 = _parser.Parse("kwart over vijf");
        System.Diagnostics.Debug.Assert(result5 == new TimeSpan(5, 15, 0), 
            $"'kwart over vijf' should be 5:15, got {result5}");

        // Test case 6: "half vijf" should parse to 4:30 (Dutch: 30 minutes BEFORE 5)
        var result6 = _parser.Parse("half vijf");
        System.Diagnostics.Debug.Assert(result6 == new TimeSpan(4, 30, 0), 
            $"'half vijf' should be 4:30, got {result6}");

        // Test case 7: "5 voor half twaalf" should parse to 11:25 (compound expression!)
        var result7 = _parser.Parse("5 voor half twaalf");
        System.Diagnostics.Debug.Assert(result7 == new TimeSpan(11, 25, 0), 
            $"'5 voor half twaalf' should be 11:25, got {result7}");

        // Test case 8: "vijf over half twee" should parse to 1:35
        var result8 = _parser.Parse("vijf over half twee");
        System.Diagnostics.Debug.Assert(result8 == new TimeSpan(1, 35, 0), 
            $"'vijf over half twee' should be 1:35, got {result8}");

        // Test case 9: "vijf na half drie" should parse to 2:35 ? IMPORTANT!
        var result9 = _parser.Parse("vijf na half drie");
        System.Diagnostics.Debug.Assert(result9 == new TimeSpan(2, 35, 0), 
            $"'vijf na half drie' should be 2:35, got {result9}");

        // Test case 10: "tien voor half acht" should parse to 7:20
        var result10 = _parser.Parse("tien voor half acht");
        System.Diagnostics.Debug.Assert(result10 == new TimeSpan(7, 20, 0), 
            $"'tien voor half acht' should be 7:20, got {result10}");

        System.Diagnostics.Debug.WriteLine("? All Dutch parser tests passed!");
    }
}
