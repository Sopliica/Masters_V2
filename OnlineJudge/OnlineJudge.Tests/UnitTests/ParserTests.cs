using Xunit;
using FluentAssertions;
using OnlineJudge.Parsing;
using NSubstitute.ExceptionExtensions;

namespace OnlineJudge.Tests.UnitTests
{
    public class ParserTests
    {
        [Fact]
        public void ParserShouldParseInputCorrectlyOrginalTest()
        {
            // Arrange
            var txt =
            @"
            TITLE: Test - When \(a \ne 0\), there are two solutions to \(ax^2 + bx + c = 0\) and they are
              \[x = {-b \pm \sqrt{b^2-4ac} \over 2a}.\]
            DESC: Test - When \(a \ne 0\)

            TIME: 10

            MEMORY: 15

            OUTPUT: [""Hello"", ""World"", ""Apple123"", ""Banana,Test""]
            
            ";

            // Act
            var parsed = new Parser().Parse(txt);

            // Assert
            parsed.Value.Output.Should().ContainInOrder("Hello", "World", "Apple123", "Banana,Test");
            parsed.Value.Output.Should().ContainEquivalentOf("         Hello     ");
            parsed.Value.Output.Should().ContainEquivalentOf("         He l  l o     ");
            parsed.Value.Output.Should().ContainEquivalentOf("       App l" + System.Environment.NewLine + " e 1 2 3    ");
            parsed.Value.Output.Should().ContainEquivalentOf("       Ban ana,Te st");
            parsed.Value.Output.Should().ContainEquivalentOf("       Ban ana\r\n,Te\n st");
            parsed.Value.Output.Should().NotContain("Ban ana,,Te st  ");
        }
        [Fact]
        public void ParserShoulThrowExecptionWhenThereIsNoTitleProperty()
        {

            var txt =
            @"
            DESC: Test - When \(a \ne 0\)

            TIME: 10

            MEMORY: 15

            OUTPUT: [""Hello"", ""World"", ""Apple123"", ""Banana,Test""]
            
            ";

            var parser = new Parser().Parse(txt);

            new Action(() => new Parser().Parse(txt)).Should().Throw<Exception>();
        }
    }
}
