namespace OnlineJudge.Models.IO
{
    public class GodboltOutput
    {
        public string inputFilename { get; set; }
        public int code { get; set; }
        public bool okToCache { get; set; }
        public bool timedOut { get; set; }
        public object[] stdout { get; set; }
        public object[] stderr { get; set; }
        public string execTime { get; set; }
        public float processExecutionResultTime { get; set; }
        public string[] compilationOptions { get; set; }
        public object[] downloads { get; set; }
        public object[] tools { get; set; }
        public Asm[] asm { get; set; }
        public Labeldefinitions labelDefinitions { get; set; }
        public string parsingTime { get; set; }
        public int filteredCount { get; set; }
        public Populararguments popularArguments { get; set; }
        public Execresult? execResult { get; set; }
    }

    public class Labeldefinitions
    {
    }

    public class Populararguments
    {
    }

    public class Execresult
    {
        public int code { get; set; }
        public bool okToCache { get; set; }
        public bool timedOut { get; set; }
        public Stdout[] stdout { get; set; }
        public object[] stderr { get; set; }
        public string execTime { get; set; }
        public float processExecutionResultTime { get; set; }
        public bool didExecute { get; set; }
        public Buildresult buildResult { get; set; }
    }

    public class Buildresult
    {
        public string inputFilename { get; set; }
        public int code { get; set; }
        public bool okToCache { get; set; }
        public bool timedOut { get; set; }
        public object[] stdout { get; set; }
        public object[] stderr { get; set; }
        public string execTime { get; set; }
        public float processExecutionResultTime { get; set; }
        public object[] downloads { get; set; }
        public string executableFilename { get; set; }
        public string[] compilationOptions { get; set; }
    }

    public class Stdout
    {
        public string text { get; set; }
    }

    public class Asm
    {
        public string text { get; set; }
        public object source { get; set; }
        public object[] labels { get; set; }
    }

}
