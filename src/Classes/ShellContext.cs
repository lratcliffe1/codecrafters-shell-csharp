namespace src.Classes;

public class ShellContext
{
    public required string RawInput { get; set; }

    public required string Command { get; set; }

    public required List<string> Parameters { get; set; }

    public required string WorkingDirectory { get; set; }

    public OutputType? OutputType { get; set; }

    public string? OutputTarget { get; set; }
    
    public string? Output { get; set; }

    public string? ErrorTarget { get; set; }
    
    public string? Error { get; set; }
}

public enum OutputType
{
    None = 0,
    Redirect = 1,
    Append = 2,
}