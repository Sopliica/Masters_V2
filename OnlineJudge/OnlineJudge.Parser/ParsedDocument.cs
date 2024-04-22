namespace OnlineJudge.Parsing;

public class ParsedDocument
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int MemoryLimitMB { get; set; }
    public string TestCases { get; set; }
}