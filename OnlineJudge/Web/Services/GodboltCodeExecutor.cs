using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using OnlineJudge.Models.Domain;
using OnlineJudge.Models.IO;
using OnlineJudge.Services;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OnlineJudge.Services.Tests
{
    public class GodboltCodeExecutorTests
    {
        [Fact]
        public async Task TryExecute_Should_Return_Success_Result_When_Response_Is_Successful_And_Execution_Is_Successful()
        {
            // Arrange
            var expectedResult = new SubmissionResult(ExecutionStatusEnum.Success, "Output", 100);
            var restResponse = new RestResponse
            {
                Content = JsonConvert.SerializeObject(new GodboltOutput
                {
                    execResult = new Execresult
                    {
                        code = 0,
                        stdout = new[] { new Stdout { text = "Output" } },
                        execTime = "100"
                    }
                }),
                StatusCode = System.Net.HttpStatusCode.OK
            };

            var restClient = Substitute.For<IRestClient>();
            restClient.PostAsync(Arg.Any<IRestRequest>()).Returns(Task.FromResult(restResponse));

            var memoryCache = Substitute.For<IMemoryCache>();

            var executor = new GodboltCodeExecutor(memoryCache) { _client = restClient };

            // Act
            var result = await executor.TryExecute("lang", "compiler", "code", new List<SubmissionLibrary>());

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task TryExecute_Should_Return_Failure_Result_When_Response_Is_Not_Successful()
        {
            // Arrange
            var restResponse = new RestResponse { StatusCode = System.Net.HttpStatusCode.BadRequest };
            var restClient = Substitute.For<IRestClient>();
            restClient.PostAsync(Arg.Any<IRestRequest>()).Returns(Task.FromResult(restResponse));

            var memoryCache = Substitute.For<IMemoryCache>();

            var executor = new GodboltCodeExecutor(memoryCache) { _client = restClient };

            // Act
            var result = await executor.TryExecute("lang", "compiler", "code", new List<SubmissionLibrary>());

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task GetLibraries_Should_Return_Success_Result_When_Response_Is_Successful()
        {
            // Arrange
            var expectedOutput = new List<LibraryDetails>();

            var restResponse = new RestResponse
            {
                Content = JsonConvert.SerializeObject(new List<LibraryInfo>()),
                StatusCode = System.Net.HttpStatusCode.OK
            };

            var restClient = Substitute.For<IRestClient>();
            restClient.GetAsync<List<LibraryInfo>>(Arg.Any<IRestRequest>()).Returns(Task.FromResult(restResponse));

            var memoryCache = Substitute.For<IMemoryCache>();

            var executor = new GodboltCodeExecutor(memoryCache) { _client = restClient };

            // Act
            var result = await executor.GetLibraries("lang");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedOutput);
        }

        [Fact]
        public async Task GetLangsAndCompilers_Should_Return_Success_Result_When_Response_Is_Successful()
        {
            // Arrange
            var expectedOutput = new List<LanguageDetails>();

            var restResponse = new RestResponse
            {
                Content = JsonConvert.SerializeObject(new List<GodboltCompilersAndLangsList>()),
                StatusCode = System.Net.HttpStatusCode.OK
            };

            var restClient = Substitute.For<IRestClient>();
            restClient.GetAsync<List<GodboltCompilersAndLangsList>>(Arg.Any<IRestRequest>()).Returns(Task.FromResult(restResponse));

            var memoryCache = Substitute.For<IMemoryCache>();

            var executor = new GodboltCodeExecutor(memoryCache) { _client = restClient };

            // Act
            var result = await executor.GetLangsAndCompilers();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedOutput);
        }
    }
}
