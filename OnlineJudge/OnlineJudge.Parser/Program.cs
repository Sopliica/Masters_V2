
using OnlineJudge.Parsing;

var txt = File.ReadAllText("input.txt");

var parsed = new Parser().Parse(txt);

if (parsed.Success)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"'{parsed.Value.Tite}'");
    Console.WriteLine($"'{parsed.Value.Description}'");
    Console.WriteLine(parsed.Value.TimeLimitSeconds);
    Console.WriteLine(parsed.Value.MemoryLimitMB);
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Parsing Failed");
    Console.WriteLine(parsed.Error);
}

Console.ResetColor();