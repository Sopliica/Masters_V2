namespace OnlineJudge.Models.IO
{
    public class LibraryInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public Version[] versions { get; set; }
        public string description { get; set; }
    }

    public class Version
    {
        public string version { get; set; }
        public string[] staticliblink { get; set; }
        public string[] alias { get; set; }
        public string[] dependencies { get; set; }
        public string[] path { get; set; }
        public string[] libpath { get; set; }
        public string[] liblink { get; set; }
        public string[] options { get; set; }
        public bool hidden { get; set; }
        public int order { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string lookupversion { get; set; }
    }

}
