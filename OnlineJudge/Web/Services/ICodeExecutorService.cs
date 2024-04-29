using OnlineJudge.Miscs;
using OnlineJudge.Models.Miscs;
using OnlineJudge.Models.Domain;

namespace OnlineJudge.Services
{
    public interface ICodeExecutorService
    {
        public Task<Result<SubmissionResult>> TryExecute(
            string lang,
            string compiler,
            string code,
            List<SubmissionLibrary> libraries);

        public Task<Result<List<LanguageDetails>>> GetLangsAndCompilers();

        public Task<Result<List<LibraryDetails>>> GetLibraries(string lang);
    }
}