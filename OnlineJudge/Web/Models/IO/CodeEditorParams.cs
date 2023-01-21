using OnlineJudge.Miscs;

namespace OnlineJudge.Models.IO
{
    public class CodeEditorParams
    {
        public Guid AssignmentId { get; set; }

        public List<LanguageDetails> AvailableLanguages { get; set; } = new List<LanguageDetails>();
    }
}
