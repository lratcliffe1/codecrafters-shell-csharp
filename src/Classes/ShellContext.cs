namespace src.Classes;

public class ShellContext
{
    public required string RawInput { get; set; }

    public required string Command { get; set; }

    public required List<string> Parameters { get; set; }

    public string? OutputTarget { get; set; }
    
    public string? Output { get; set; }

    public string? ErrorTarget { get; set; }
    
    public string? Error { get; set; }
}