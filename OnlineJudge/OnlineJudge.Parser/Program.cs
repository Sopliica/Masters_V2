
using OnlineJudge.Parsing;

var txt = File.ReadAllText("input.txt");

var parsed = new Parser().Parse(txt);

if (parsed.Success)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{parsed.Value.Title}");
    Console.WriteLine("____________________");
    Console.WriteLine($"{parsed.Value.Description}");
    Console.WriteLine("____________________");
    Console.WriteLine(parsed.Value.TimeLimitSeconds);
    Console.WriteLine("____________________");
    Console.WriteLine(parsed.Value.MemoryLimitMB);
    Console.WriteLine("____________________");
    Console.WriteLine("TestCases:");
    Console.WriteLine(parsed.Value.TestCases);
    Console.WriteLine("____________________");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Parsing Failed");
    Console.WriteLine(parsed.Error);
}

Console.ResetColor();