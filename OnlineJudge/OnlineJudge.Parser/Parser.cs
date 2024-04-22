using System.Text;
using OnlineJudge.Miscs;

namespace OnlineJudge.Parsing;

public class Parser
{
    private const string TITLE_SYNTAX = "TITLE: ";
    private const string DESCRIPTION_SYNTAX = "DESC: ";
    private const string TIME_SYNTAX = "TIME: ";
    private const string MEMORY_SYNTAX = "MEMORY: ";
    private const string TESTCASES_SYNTAX = "TESTCASES:";

    private StringBuilder _titleBuilder = new();
    private StringBuilder _descriptionBuilder = new();
    private StringBuilder _testCasesBuilder = new();

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

            var stateFullyHandled = HandleState(state);

            if (stateFullyHandled)
                state = ParsingState.None;
        } while (_Index++ < _Lines.Count - 1);

        var result = Validate();

        if (!result.Success)
            return Result.Fail<ParsedDocument>(result.Error);

        var doc = new ParsedDocument
        {
            Title = _titleBuilder.ToString().TrimEnd(),
            Description = _descriptionBuilder.ToString().TrimEnd(),
            TimeLimitSeconds = _timeLimit.Value,
            MemoryLimitMB = _memoryLimit.Value,
            TestCases = _testCasesBuilder.ToString()
        };

        return Result.Ok(doc);
    }

    private Result Validate()
    {
        if (_timeLimit == null)
            _Errors.Add($"Time limit was not specified. Define time limit using \"{TIME_SYNTAX}\" syntax.");

        if (_memoryLimit == null)
            _Errors.Add($"Memory limit was not specified. Define memory limit using \"{MEMORY_SYNTAX}\" syntax.");

        if (_titleBuilder.Length == 0)
            _Errors.Add($"Title was not specified. Define Title using \"{TITLE_SYNTAX}\" syntax.");

        if (_descriptionBuilder.Length == 0)
            _Errors.Add($"Description was not specified. Define description using \"{DESCRIPTION_SYNTAX}\" syntax.");

        if (_testCasesBuilder.Length == 0) // Validation for test cases
            _Errors.Add($"Test cases were not specified. Define test cases using \"{TESTCASES_SYNTAX}\" syntax.");

        return Result.Ok();
    }

    private bool HandleState(ParsingState state)
    {
        var line = _Lines[_Index];

        switch (state)
        {
            case ParsingState.Title:
            {
                _titleBuilder.AppendLine(line);
                return false;
            }
            case ParsingState.Description:
            {
                _descriptionBuilder.AppendLine(line);
                return false;
            }
            case ParsingState.TestCases: // Added case for test cases
            {
                _testCasesBuilder.AppendLine(line);
                return false;
            }
            case ParsingState.TimeLimit:
            {
                if (string.IsNullOrWhiteSpace(line.Trim()))
                    return false;

                if (int.TryParse(line, out var val))
                {
                    _timeLimit = val;
                }
                else
                {
                    _Errors.Add($"Cannot read Time Limit's value as int. Value: '{line}'.");
                }
                return true;
            }
            case ParsingState.MemoryLimit:
            {
                if (string.IsNullOrWhiteSpace(line.Trim()))
                    return false;

                if (int.TryParse(line, out var val))
                {
                    _memoryLimit = val;
                }
                else
                {
                    _Errors.Add($"Cannot read Memory Limit value as int. Value: '{line}'.");
                }

                return true;
            }
            case ParsingState.None:
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _Errors.Add($"Value with unknown section - {line}");
                }
                return true;
            }
        }

        throw new Exception("unsupported state");
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
        else if (line.StartsWith(TESTCASES_SYNTAX)) // Added detection logic for test cases
        {
            return Result.Ok((ParsingState.TestCases, RemoveSyntax(line, TESTCASES_SYNTAX)));
        }

        return Result.Fail<(ParsingState NewState, string LineWithoutItsSyntax)>("State change not detected");
    }

    private string RemoveSyntax(string line, string syntax)
    {
        return line.Substring(syntax.Length);
    }

    private static List<string> SplitToLines(string s)
    {
        return s.ReplaceLineEndings()
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .ToList();
    }
}
