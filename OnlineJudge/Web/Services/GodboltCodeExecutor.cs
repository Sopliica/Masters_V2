using RestSharp;
using Newtonsoft.Json;
using OnlineJudge.Miscs;
using OnlineJudge.Models.IO;
using OnlineJudge.Models.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace OnlineJudge.Services
{
    public class GodboltCodeExecutor : ICodeExecutorService
    {
        private RestClient _client = new RestClient("https://godbolt.org/");
        private readonly IMemoryCache _cache;

        public GodboltCodeExecutor(IMemoryCache cache)
        {
            this._cache = cache;
        }

        public async Task<Result<SubmissionResult>> TryExecute(string lang, string compiler, string code)
        {
            var requestData = new GodboltRequest
            {
                lang = lang,
                allowStoreCodeDebug = true,
                source = code,
                options = new Options
                {
                    userArguments = "",
                    compilerOptions = new Compileroptions
                    {
                    },
                    tools = Array.Empty<Tool>(),
                    filters = new Filters
                    {
                        execute = true
                    },
                    libraries = Array.Empty<Library>()
                }
            };

            var request = new RestRequest($"/api/compiler/{compiler}/compile", Method.Post);
            request.AddJsonBody(requestData);

            var response = await _client.PostAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var output = JsonConvert.DeserializeObject<GodboltOutput>(response.Content);
                Console.WriteLine(JsonConvert.SerializeObject(output, Formatting.Indented));

                if (output.execResult.code == 0)
                {
                    var stdout = string.Join(",", output.execResult.stdout.Select(x => x.text));
                    return Result.Ok(new SubmissionResult(stdout, Convert.ToInt32(output.execResult.execTime)));
                }
                else
                {
                    var errMessage = $"Error: {string.Join(",", output.execResult.stderr) + string.Join(",", output.stderr)}";
                    return new Result<SubmissionResult>(new SubmissionResult(errMessage, 0), false, errMessage);
                }
            }
            else
            {
                Console.WriteLine(response.Content);
                return Result.Fail<SubmissionResult>("Network error");
            }
        }

        public async Task<Result<List<LanguageDetails>>> GetLangsAndCompilers()
        {
            const string cacheKey = "LanguagesGodboltCache";
            if (_cache.TryGetValue<List<LanguageDetails>>(cacheKey, out var list))
                return Result.Ok(list);

            var langsRequest = new RestRequest($"https://godbolt.org/api/compilers?fields=id,name,lang", Method.Get);
            langsRequest.AddHeader("Accept", "application/json");

            var langsResponse = await _client.GetAsync<List<GodboltCompilersAndLangsList>>(langsRequest);

            if (langsResponse == null)
            {
                return Result.Fail<List<LanguageDetails>>("Unable to load languages list");
            }

            var mapped = langsResponse.Select(x => new LanguageDetails { CompilerName = x.name, LanguageName = x.lang, CompilerId = x.id }).ToList();
            _cache.Set(cacheKey, mapped, TimeSpan.FromHours(24));
            return Result.Ok(mapped);
        }
    }
}
