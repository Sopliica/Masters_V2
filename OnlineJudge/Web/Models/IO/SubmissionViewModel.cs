using OnlineJudge.Models.Domain;

namespace OnlineJudge.Models.IO
{
    public class SubmissionViewModel
    {
        public Submission Submission { get; set; }

        public bool IsOutputOK { get; set; }
    }
}
