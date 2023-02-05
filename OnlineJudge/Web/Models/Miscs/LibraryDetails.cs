namespace OnlineJudge.Models.Miscs
{
    public class LibraryDetails
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<string> Versions { get; set; } = new List<string>();
    }
}
