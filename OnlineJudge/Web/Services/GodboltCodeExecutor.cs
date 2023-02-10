using RestSharp;
using Newtonsoft.Json;
using OnlineJudge.Miscs;
using OnlineJudge.Models.IO;
using OnlineJudge.Models.Domain;
using Microsoft.Extensions.Caching.Memory;
using OnlineJudge.Models.Miscs;
using Serilog;

namespace OnlineJudge.Services
{
    public class GodboltCodeExecutor : ICodeExecutorService
    {
        private RestClient _client = new RestClient("https://godbolt.org/");
        private readonly IMemoryCache _cache;

        public GodboltCodeExecutor(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<Result<SubmissionResult>> TryExecute(string lang, string compiler, string code, List<SubmissionLibrary> libraries)
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
                    libraries = libraries.Select(x => new Library { id = x.LibraryId, version = x.LibraryVersionId }).ToArray()
                }
            };

            Log.Logger.Information(JsonConvert.SerializeObject(requestData, Formatting.Indented));

            var request = new RestRequest($"/api/compiler/{compiler}/compile", Method.Post);
            request.AddJsonBody(requestData);

            var response = await _client.PostAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var output = JsonConvert.DeserializeObject<GodboltOutput>(response.Content);
                Log.Logger.Information(JsonConvert.SerializeObject(output, Formatting.Indented));

                if (output.execResult.code == 0)
                {
                    var stdout = string.Join(",", output.execResult.stdout.Select(x => x.text));
                    return Result.Ok(new SubmissionResult(ExecutionStatusEnum.Success, stdout, Convert.ToInt32(output.execResult.execTime)));
                }
                else
                {
                    var errMessage = $"Error: {string.Join(",", output.execResult.stderr) + string.Join(",", output.stderr)}";
                    return Result.Ok(new SubmissionResult(ExecutionStatusEnum.Failed, errMessage, Convert.ToInt32(output.execResult.execTime)));
                }
            }
            else
            {
                Log.Logger.Information(response.Content);
                return Result.Fail<SubmissionResult>("Network error");
            }
        }

        public async Task<Result<List<LibraryDetails>>> GetLibraries(string lang)
        {
            var cacheKey = $"LibrariesGodboltCache_{lang}";
            if (_cache.TryGetValue<List<LibraryDetails>>(cacheKey, out var list))
            {
                Console.WriteLine($"Using Library Cache: {lang}");
                return Result.Ok(list);
            }

            var libsRequest = new RestRequest($"https://godbolt.org/api/libraries/" + lang, Method.Get);
            libsRequest.AddHeader("Accept", "application/json");
            var libsResponse = await _client.GetAsync<List<LibraryInfo>>(libsRequest);

            var output = libsResponse
                         .Select(x =>
                         new LibraryDetails
                         {
                             Name = x.name,
                             Id = x.id,
                             Versions = x.versions
                             .Select(v => new VersionDetails { VersionName = v.version, VersionId = v.id, Pathes = v.path.ToList() })
                             .ToList()
                         })
                         .ToList();

            _cache.Set(cacheKey, output, TimeSpan.FromHours(72));
            return Result.Ok(output);
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
