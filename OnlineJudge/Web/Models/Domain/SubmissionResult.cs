namespace OnlineJudge.Models.Domain
{
    public class SubmissionResult
    {
        public SubmissionResult()
        {

        }

        public SubmissionResult(ExecutionStatusEnum executionStatus, string output, int time, int lp)
        {
            Output = output;
            Time = time;
            ExecutionStatus = executionStatus;
            Lp = lp;
        }

        public Guid SubmissionResultId { get; set; } = Guid.NewGuid();
        public string Output { get; set; }
        public int Time { get; set; }
        public int AttemptedExecutionsCount { get; set; } = 0;
        public ExecutionStatusEnum ExecutionStatus { get; set; }
        public Guid SubmissionId { get; set; }
        public Submission Submission { get; set; }
        public int Lp { get; set; }
    }
}