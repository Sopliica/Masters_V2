namespace OnlineJudge.Services
{
    public static class OutputComparer
    {
        public static bool Compare(string output, List<string> expectedOutputs)
        {
            foreach (var expected in expectedOutputs)
            {
                if (Compare(output, expected))
                    return true;
            }

            return false;
        }

        private static bool Compare(string output, string expected)
        {
            if (output == expected)
                return true;

            var noWhiteCharsOutput = output.Where(x => !char.IsWhiteSpace(x)).ToList();
            var noWhiteCharsExpected = expected.Where(x => !char.IsWhiteSpace(x)).ToList();

            if (noWhiteCharsOutput.SequenceEqual(noWhiteCharsExpected))
                return true;

            return false;
        }
    }
}
