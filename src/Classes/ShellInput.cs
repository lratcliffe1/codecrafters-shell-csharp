namespace src.Classes;

public class ShellInput
{
    public required string Input { get; set; }

    public required string Command { get; set; }

    public required List<string> Parameters { get; set; }
}