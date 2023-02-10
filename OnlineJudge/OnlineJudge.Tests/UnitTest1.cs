using OnlineJudge.Parsing;
using OnlineJudge.Services;

namespace OnlineJudge.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var txt =
            """
                        
            TITLE: Test - When \(a \ne 0\), there are two solutions to \(ax^2 + bx + c = 0\) and they are
              \[x = {-b \pm \sqrt{b^2-4ac} \over 2a}.\]
            DESC: Test - When \(a \ne 0\)

            TIME: 10

            MEMORY: 15

            OUTPUT: ["Hello", "World", "Apple123", "Banana,Test"]
            
            """;
            var parsed = new Parser().Parse(txt);

            Assert.True(OutputComparer.Compare("Hello", parsed.Value.Output));
            Assert.True(OutputComparer.Compare("World", parsed.Value.Output));
            Assert.True(OutputComparer.Compare("         Hello     ", parsed.Value.Output));
            Assert.True(OutputComparer.Compare("         He l  l o     ", parsed.Value.Output));
            Assert.True(OutputComparer.Compare("       App l" + Environment.NewLine + " e 1 2 3    ", parsed.Value.Output));
            Assert.True(OutputComparer.Compare("       Ban ana,Te st", parsed.Value.Output));
            Assert.True(OutputComparer.Compare("       Ban ana\r\n,Te\n st", parsed.Value.Output));
            Assert.False(OutputComparer.Compare("Ban ana,,Te st  ", parsed.Value.Output));
        }
    }
}