using src.Helpers;

class AutoCompletionHandler : IAutoCompleteHandler
{
  public char[] Separators { get; set; } = "abcdefghijklmnopqrstuvwxyz_".ToArray();

  private readonly string[] _commands = CommandConstants.All;
  private string _lastPrefix = "";
  private int _tabCount = 0;

  public string[] GetSuggestions(string text, int index)
  {
    if (string.IsNullOrWhiteSpace(text))
      return null!;

    var matches = _commands
        .Concat(FileExecuter.FindExecutablesAtPath())
        .Where(c => c.StartsWith(text, StringComparison.OrdinalIgnoreCase))
        .Distinct()
        .OrderBy(c => c)
        .ToList();

    if (matches.Count == 0)
    {
      _tabCount = 0;
      Console.Write("\x07");
      return null!;
    }

    if (matches.Count == 1)
    {
      _tabCount = 0;
      return [$"{matches[0][text.Length..]} "];
    }

    if (text == _lastPrefix)
    {
      _tabCount++;
    }
    else
    {
      _lastPrefix = text;
      _tabCount = 1;
    }

    if (_tabCount == 1)
    {
      string foundMatchingPrefix = GetLongestCommonPrefix(matches);

      if (foundMatchingPrefix == text)
      {
        Console.Write("\x07");
        return null!;
      }

      _tabCount = 0;
      return [$"{foundMatchingPrefix[text.Length..]}"];
    }
    else
    {
      Console.WriteLine();
      Console.WriteLine(string.Join("  ", matches));
      Console.Write($"$ {text}");

      _tabCount = 0;
      return null!;
    }
  }

  public void Reset()
  {
    _lastPrefix = "";
    _tabCount = 0;
  }

  private static string GetLongestCommonPrefix(List<string> matches)
  {
    if (matches == null || matches.Count == 0)
      return string.Empty;

    string prefix = matches[0];

    for (int i = 1; i < matches.Count; i++)
    {
      string currentMatch = matches[i];

      int minLength = Math.Min(prefix.Length, currentMatch.Length);
      int commonLength = 0;

      while (commonLength < minLength && char.ToLowerInvariant(prefix[commonLength]) == char.ToLowerInvariant(currentMatch[commonLength]))
        commonLength++;

      prefix = prefix[..commonLength];

      if (prefix.Length == 0)
        break;
    }

    return prefix;
  }
}
