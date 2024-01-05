namespace OnlineJudge.Parsing
{
    public class ParsedDocument
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Output { get; set; } = new List<string>();

        public int TimeLimitSeconds { get; set; }

        public int MemoryLimitMB { get; set; }
        public string? CodeSample { get; set; }
    }
}