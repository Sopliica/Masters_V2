using FluentAssertions;
using NSubstitute;
using OnlineJudge.Services;
using System.Collections.Generic;
using Xunit;

namespace OnlineJudge.Tests.UnitTests;

public class OutputComparerTests
{
    [Fact]
    public void CompareShouldReturnTrueWhenOutputMatchesExpected()
    {
        var output = "Hello World";
        var expectedOutputs = new List<string> { "Hello World" };

        var result = OutputComparer.Compare(output, expectedOutputs);

        result.Should().BeTrue();
    }

    [Fact]
    public void CompareShouldReturnTrueWhenOutputMatchesExpectedIgnoringWhitespace()
    {
        var output = "Hello   World";
        var expectedOutputs = new List<string> { "Hello World" };

        var result = OutputComparer.Compare(output, expectedOutputs);

        result.Should().BeTrue();
    }

    [Fact]
    public void CompareShouldReturnFalseWhenOutputDoesNotMatchAnyExpected()
    {
        var output = "Hello World";
        var expectedOutputs = new List<string> { "Goodbye World", "Hello Universe" };

        var result = OutputComparer.Compare(output, expectedOutputs);

        result.Should().BeFalse();
    }

    [Fact]
    public void CompareShouldReturnFalseWhenOutputDoesNotMatchIgnoringWhitespace()
    {
        var output = "Hello World";
        var expectedOutputs = new List<string> { "HelloWorld" };

        var result = OutputComparer.Compare(output, expectedOutputs);

        result.Should().BeFalse();
    }

    [Fact]
    public void CompareShouldReturnFalseWhenOutputIsNull()
    {
        string output = null;
        var expectedOutputs = new List<string> { "Hello World" };

        var result = OutputComparer.Compare(output, expectedOutputs);

        result.Should().BeFalse();
    }

    [Fact]
    public void CompareShouldReturnFalseWhenExpectedOutputsIsNull()
    {
        var output = "Hello World";
        List<string> expectedOutputs = null;

        var result = OutputComparer.Compare(output, expectedOutputs);

        result.Should().BeFalse();
    }
}
