namespace OnlineJudge.Models.Domain
{
    public class SubmissionResult
    {
        public SubmissionResult()
        {

        }

        public SubmissionResult(string output, int time)
        {
            Output = output;
            Time = time;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Output { get; set; }

        public int Time { get; set; }
    }
}