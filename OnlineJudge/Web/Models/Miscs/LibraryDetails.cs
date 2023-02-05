namespace OnlineJudge.Models.Miscs
{
    public class LibraryDetails
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<VersionDetails> Versions { get; set; } = new List<VersionDetails>();
    }
}
