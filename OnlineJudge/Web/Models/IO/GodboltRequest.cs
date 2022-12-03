namespace OnlineJudge.Models.IO
{
    public class GodboltRequest
    {
        public string source { get; set; }
        public Options options { get; set; }
        public string lang { get; set; }
        public bool allowStoreCodeDebug { get; set; }
    }

    public class Options
    {
        public string userArguments { get; set; }
        public Compileroptions compilerOptions { get; set; }
        public Filters filters { get; set; }
        public Tool[] tools { get; set; }
        public Library[] libraries { get; set; }
    }

    public class Compileroptions
    {
        public bool skipAsm { get; set; }
        public bool executorRequest { get; set; }
    }

    public class Filters
    {
        public bool binary { get; set; }
        public bool commentOnly { get; set; }
        public bool demangle { get; set; }
        public bool directives { get; set; }
        public bool execute { get; set; }
        public bool intel { get; set; }
        public bool labels { get; set; }
        public bool libraryCode { get; set; }
        public bool trim { get; set; }
    }

    public class Tool
    {
        public string id { get; set; }
        public string args { get; set; }
    }

    public class Library
    {
        public string id { get; set; }
        public string version { get; set; }
    }

}
