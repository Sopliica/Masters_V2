using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Newtonsoft.Json;
using OnlineJudge.Models.Domain;
using OnlineJudge.Models.IO;
using OnlineJudge.Models.Miscs;
using OnlineJudge.Services;
using Xunit;

namespace YourNamespace.Tests
{
    public class GodboltCodeExecutorTests
    {
        private readonly GodboltCodeExecutor _godboltCodeExecutor;
        private readonly Mock<IMemoryCache> _memoryCacheMock = new Mock<IMemoryCache>();

        public GodboltCodeExecutorTests()
        {
            _godboltCodeExecutor = new GodboltCodeExecutor(_memoryCacheMock.Object);
        }

        [Fact]
        public async Task TryExecuteSuccessfulExecutionReturnsSuccessResult()
        {
            var lang = "C++";
            var compiler = "gcc";
            var code = "int main() { return 0; }";
            var libraries = new List<SubmissionLibrary>();
            var successfulResponseContent = JsonConvert.SerializeObject(new GodboltOutput
            {
                execResult = new Execresult
                {
                    code = 0,
                    stdout = new [] { new Stdout { text = "Hello" } },
                    execTime = "100"
                }
            });
            var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(successfulResponseContent)
            };
            var httpClientMock = new Mock<HttpClient>();
            httpClientMock
                .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(successfulResponse);


            var result = await _godboltCodeExecutor.TryExecute(lang, compiler, code, libraries);

            result.Success.Should().BeTrue(); 
            result.Value.ExecutionStatus.Should().Be(ExecutionStatusEnum.Success); 
            result.Value.Output.Should().Be("Hello"); 
            result.Value.Time.Should().Be(100); 
        }

        [Fact]
        public async Task GetLibrariesCacheContainsDataReturnsCachedData()
        {
            var lang = "C++";
            var cachedData = new List<LibraryDetails> { new LibraryDetails { Name = "lib1", Id = "1" } };
            _memoryCacheMock.
                Setup(cache => cache.TryGetValue<List<LibraryDetails>>
                (It.IsAny<object>(), out cachedData)).Returns(true);

            var result = await _godboltCodeExecutor.GetLibraries(lang);

            result.Success.Should().BeTrue(); 
            result.Value.Should().BeEquivalentTo(cachedData); 
        }
    }
}
