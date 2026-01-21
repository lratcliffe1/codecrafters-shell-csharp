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

    return _commands.Where(c => c.StartsWith(text))
      .Select(c => c[text.Length..] + " ")
      .ToArray();
  }
}
