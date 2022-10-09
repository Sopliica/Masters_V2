namespace OnlineJudge.Models.Domain
{
    public class Assignment
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Tite { get; set; }

        public string Description { get; set; }

        public int TimeLimitSeconds { get; set; }

        public int MemoryLimitMB { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
