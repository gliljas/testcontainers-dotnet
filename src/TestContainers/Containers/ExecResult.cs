namespace TestContainers
{
    public class ExecResult
    {
        public string Stdout { get; internal set; }
        public string Stderr { get; internal set; }
        public long ExitCode { get; internal set; }
    }
}
