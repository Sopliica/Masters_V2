namespace OnlineJudge.Models.Domain
{
    public class AssignmentOutput
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Text { get; set; }
    }
}