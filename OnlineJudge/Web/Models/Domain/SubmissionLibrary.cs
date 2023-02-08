namespace OnlineJudge.Models.Domain
{
    public class SubmissionLibrary
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string LibraryId { get; set; }

        public string LibraryName { get; set; }

        public string LibraryVersion { get; set; }

        public string LibraryVersionId { get; set; }
    }
}
