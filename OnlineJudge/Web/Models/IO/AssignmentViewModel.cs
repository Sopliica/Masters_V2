using OnlineJudge.Miscs;

namespace OnlineJudge.Models.Domain
{
    public class AssignmentViewModel
    {
        public AssignmentViewModel()
        {

        }

        public AssignmentViewModel(Assignment value)
        {
            Id = value.Id;
            Title = value.Title;
            Description = value.Description;
            TimeLimitSeconds = value.TimeLimitSeconds;
            MemoryLimitMB = value.MemoryLimitMB;
            IsDeleted = value.IsDeleted;
        }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int TimeLimitSeconds { get; set; }

        public int MemoryLimitMB { get; set; }

        public bool IsDeleted { get; set; }

        public List<LanguageDetails> AvailableLanguages { get; set; } = new List<LanguageDetails>();
    }
}
