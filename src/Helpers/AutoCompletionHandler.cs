using src.Helpers;

class AutoCompletionHandler : IAutoCompleteHandler
{
  public char[] Separators { get; set; }

  public AutoCompletionHandler() {
    Separators = "abcdefghijklmnopqrstuvwxyz".ToArray();
  }
  
  private readonly string[] _commands = ["exit", "echo"];

  public string[] GetSuggestions(string text, int index)
  {
    if (string.IsNullOrWhiteSpace(text))
      return null!;

    string[] foundCommands = 
    [
      ..GetCommandSuggestions(text),
      ..GetExternalCommandSuggestions(text),
    ];

    return foundCommands.Any() ? foundCommands : ["\x07"];
  }

  private string[] GetCommandSuggestions(string text)
  {
    return _commands.Where(c => c.StartsWith(text))
      .Select(c => c[text.Length..] + " ")
      .ToArray();
  }

  private static string[] GetExternalCommandSuggestions(string text)
  {
    return FileExecuter.FindExecutablesAtPath()
      .Where(c => c.StartsWith(text))
      .Select(c => c[text.Length..] + " ")
      .ToArray();
  }
}
