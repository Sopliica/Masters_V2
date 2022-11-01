namespace OnlineJudge.Models.IO
{
    public class SubmissionInput
    {
        public Guid AssignmentId { get; set; }

        public string Language { get; set; }

        public string Code { get; set; }
    }
}
