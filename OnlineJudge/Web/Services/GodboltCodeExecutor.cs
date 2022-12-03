using Newtonsoft.Json;
using OnlineJudge.Miscs;
using OnlineJudge.Models.Domain;
using OnlineJudge.Models.IO;
using RestSharp;
using System.Text.Json.Serialization;

namespace OnlineJudge.Services
{
    public class GodboltCodeExecutor : ICodeExecutorService
    {
        public async Task<Result<SubmissionResult>> TryExecute(string lang, string code)
        {
            var client = new RestClient("https://godbolt.org/");

            var comp = "dotnet700csharp";

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

            var request = new RestRequest($"/api/compiler/{comp}/compile", Method.Post);
            request.AddJsonBody(requestData);

            var response = await client.PostAsync(request);
            var output = JsonConvert.DeserializeObject<GodboltOutput>(response.Content);
            Console.WriteLine(JsonConvert.SerializeObject(output, Formatting.Indented));
            if (response.IsSuccessStatusCode)
            {
                if (output.execResult.code == 0)
                {
                    var stdout = string.Join(",", output.execResult.stdout.Select(x => x.text));
                    return Result.Ok(new SubmissionResult(stdout, Convert.ToInt32(output.execResult.execTime)));
                }
                else
                {
                    return Result.Fail<SubmissionResult>($"Error: {string.Join(",", output.execResult.stderr)}");
                }
            }
            else
            {
                return Result.Fail<SubmissionResult>("Network error");
            }
        }
    }
}
