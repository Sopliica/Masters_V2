namespace OnlineJudge.Models.IO
{
    public class SubmissionInput
    {
        public Guid AssignmentId { get; set; }

        public string Language { get; set; }

        public string Compiler { get; set; }

        public string Code { get; set; }

        public List<SubmissionLibraryInput> Libraries { get; set; } = new List<SubmissionLibraryInput>();
    }
}
