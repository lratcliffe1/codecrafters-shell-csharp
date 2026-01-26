using System.Diagnostics;

namespace src.Classes;

public class ShellContext
{
    public required string RawInput { get; set; }
    public required List<Command> Commands { get; set; }
    public required string WorkingDirectory { get; set; }
    public required List<string> History { get; set; }
    public required int HistoryAppended { get; set; }
    public required int HistoryLoaded { get; set; }
    public Stream? LastPipeReadStream { get; set; } = null;
    public List<Process> Processes { get; set; } = [];
    public List<Task> OutputTasks { get; set; } = [];

}

public class Command
{
    public required string Name { get; set; }
    public required List<string> Args { get; set; }
    public required int Index { get; set; }
    public required string StdoutTarget { get; set; }
    public string? SterrTarget { get; set; }
    public required OutputType OutputType { get; set; }
}

public enum OutputType
{
    None = 0,
    Redirect = 1,
    Append = 2,
}