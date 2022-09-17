
using OnlineJudge.Parsing;

var txt = File.ReadAllText("input.txt");

var parsed = new Parser().Parse(txt);

if (parsed.Success)
{
    Console.WriteLine(parsed.Value.Tite);
    Console.WriteLine();
    Console.WriteLine(parsed.Value.Description);
    Console.WriteLine();
    Console.WriteLine(parsed.Value.TimeLimitSeconds);
    Console.WriteLine();
    Console.WriteLine(parsed.Value.MemoryLimitMB);
}
else
{
    Console.WriteLine("Parsing Failed");
    Console.WriteLine(parsed.Error);
}