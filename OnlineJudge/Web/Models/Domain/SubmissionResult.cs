namespace OnlineJudge.Models.Domain
{
    public class SubmissionResult
    {
        public SubmissionResult()
        {

        }

        public SubmissionResult(ExecutionStatusEnum executionStatus, string output, int time)
        {
            Output = output;
            Time = time;
            ExecutionStatus = executionStatus;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Output { get; set; }

        public int Time { get; set; }

        public ExecutionStatusEnum ExecutionStatus { get; set; }
    }
}