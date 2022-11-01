namespace OnlineJudge.Models.IO
{
    public class CodeEditorParams
    {
        public Guid TaskId { get; set; }

        public List<string> AvailableLanguages { get; set; } = new List<string>();
    }
}
