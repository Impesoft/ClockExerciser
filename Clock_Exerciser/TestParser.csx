#r "Clock_Exerciser.Core/bin/Debug/net10.0/Clock_Exerciser.Core.dll"

using Clock_Exerciser.Core.Services;

var parser = new DutchTimeParser();
var result = parser.Parse("5 voor half twaalf");

Console.WriteLine($"Input: '5 voor half twaalf'");
Console.WriteLine($"Result: {result}");
Console.WriteLine($"Expected: 11:25:00");
