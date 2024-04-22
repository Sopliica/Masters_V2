using OnlineJudge.Parsing;

namespace OnlineJudge.Models.Domain
{
    public class Assignment
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Title { get; set; }

        public string Description { get; set; }

        public int TimeLimitSeconds { get; set; }

        public int MemoryLimitMB { get; set; }

        public bool IsDeleted { get; set; } = false;

        public List<TestCase> TestCases { get; set; } = new List<TestCase>();

        public List<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
