namespace OnlineJudge.Models.Miscs
{
    public class VersionDetails
    {
        public string VersionName { get; set; }

        public string VersionId { get; set; }

        public List<string> Pathes { get; set; } = new List<string>();
    }
}
