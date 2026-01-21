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

    var foundCommands = _commands.Where(c => c.StartsWith(text));

    if (!foundCommands.Any())
    {
      Console.Beep();
      return null!;
    }
    return foundCommands
      .Select(c => c[text.Length..] + " ")
      .ToArray();
  }
}
