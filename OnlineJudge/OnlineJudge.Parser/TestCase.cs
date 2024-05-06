namespace OnlineJudge.Parsing;

public class TestCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Input { get; set; }
    public string Output { get; set; }
}
