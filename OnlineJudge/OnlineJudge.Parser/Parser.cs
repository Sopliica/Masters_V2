using System.Text;
using OnlineJudge.Miscs;

namespace OnlineJudge.Parsing
{
    public class Parser
    {
        private const string TITLE_SYNTAX = "TITLE: ";
        private const string DESCRIPTION_SYNTAX = "DESC: ";
        private const string TIME_SYNTAX = "TIME: ";
        private const string MEMORY_SYNTAX = "MEMORY: ";

        private StringBuilder _titleBuilder = new StringBuilder();
        private StringBuilder _descriptionBuilder = new StringBuilder();

        private int? _timeLimit;
        private int? _memoryLimit;

        private int _Index = 0;
        private List<string> _Lines = new List<string>();
        private List<string> _Errors = new List<string>();

        public Result<ParsedDocument> Parse(string s)
        {
            _Index = 0;
            _Lines = SplitToLines(s);
            var result = GenerateDocument();
            return result;
        }

        private Result<ParsedDocument> GenerateDocument()
        {
            if (_Lines.Count == 0)
                return Result.Fail<ParsedDocument>("Document is empty");

            var state = ParsingState.None;

            do
            {
                var newState = DetectStateChange(_Lines[_Index]);

                if (newState.Success)
                {
                    state = newState.Value.NewState;
                    _Lines[_Index] = newState.Value.LineWithoutItsSyntax;
                }

                HandleState(state);
            } while (_Index++ < _Lines.Count - 1);

            var result = Validate();

            if (!result.Success)
                return Result.Fail<ParsedDocument>(result.Error);

            var doc = new ParsedDocument
            {
                Tite = _titleBuilder.ToString(),
                Description = _descriptionBuilder.ToString(),
                TimeLimitSeconds = _timeLimit.Value,
                MemoryLimitMB = _memoryLimit.Value
            };

            return Result.Ok(doc);
        }

        private Result Validate()
        {
            if (_timeLimit == null)
                _Errors.Add("Time limit was not specified");

            if (_memoryLimit == null)
                _Errors.Add("Memory limit was not specified");

            if (_titleBuilder.Length == 0)
                _Errors.Add("Title was not specified");

            if (_descriptionBuilder.Length == 0)
                _Errors.Add("Description was not specified");

            if (_Errors.Any())
                return Result.Fail(string.Join(Environment.NewLine, _Errors));

            return Result.Ok();
        }

        private void HandleState(ParsingState state)
        {
            var line = _Lines[_Index];
            switch (state)
            {
                case ParsingState.Title:
                {
                        _titleBuilder.Append(line);
                        return;
                }
                case ParsingState.Description:
                {
                        _descriptionBuilder.Append(line);
                        return;
                }
                case ParsingState.TimeLimit:
                    {
                        if (string.IsNullOrWhiteSpace(line.Trim()))
                            return;

                        if (int.TryParse(line, out var val))
                        {
                            _timeLimit = val;
                        }
                        else
                        {
                            _Errors.Add($"Cannot read Time Limit's value as int. Value: '{line}'.");
                        }
                        return;
                }
                case ParsingState.MemoryLimit:
                    {
                        if (string.IsNullOrWhiteSpace(line.Trim()))
                            return;

                        if (int.TryParse(line, out var val))
                        {
                            _memoryLimit = val;
                        }
                        else
                        {
                            _Errors.Add($"Cannot read Memory Limit value as int. Value: '{line}'.");
                        }
                        return;
                    }
                case ParsingState.None:
                {
                    return;
                }
            }
        }

        private Result<(ParsingState NewState, string LineWithoutItsSyntax)> DetectStateChange(string line)
        {
            if (line.StartsWith(TITLE_SYNTAX))
            {
                return Result.Ok((ParsingState.Title, RemoveSyntax(line, TITLE_SYNTAX)));
            }
            else if (line.StartsWith(DESCRIPTION_SYNTAX))
            {
                return Result.Ok((ParsingState.Description, RemoveSyntax(line, DESCRIPTION_SYNTAX)));
            }
            else if (line.StartsWith(TIME_SYNTAX))
            {
                return Result.Ok((ParsingState.TimeLimit, RemoveSyntax(line, TIME_SYNTAX)));
            }
            else if (line.StartsWith(MEMORY_SYNTAX))
            {
                return Result.Ok((ParsingState.MemoryLimit, RemoveSyntax(line, MEMORY_SYNTAX)));
            }

            return Result.Fail<(ParsingState NewState, string LineWithoutItsSyntax)>("State change not detected");
        }

        private string RemoveSyntax(string line, string syntax)
        {
            return line.Substring(syntax.Length);
        }

        private static List<string> SplitToLines(string s)
        {
            return s.Split
            (
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            ).ToList();
        }
    }
}
