namespace OnlineJudge.Models.Domain
{
    public class Submission
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Language { get; set; }

        public string Compiler { get; set; }

        public string Code { get; set; }

        public DateTime Submitted { get; private set; } = DateTime.Now;

        public Assignment Assignment { get; set; }

        public Guid AssignmentId { get; set; }

        public User User { get; set; }

        public Guid UserId { get; set; }

        public SubmissionResult? Result { get; set; }

        public List<SubmissionLibrary> Libraries { get; set; } = new List<SubmissionLibrary>();
    }
}
