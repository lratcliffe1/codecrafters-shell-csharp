using src.Helpers;

public sealed class AutoCompletionEngine
{
  public char[] Separators { get; set; } = "abcdefghijklmnopqrstuvwxyz_".ToArray();

  private readonly string[] _commands = CommandConstants.All;
  private string _lastPrefix = "";
  private int _tabCount = 0;

  public string? Complete(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
      return null;

    var matches = _commands
        .Concat(FileExecuter.FindExecutablesAtPath())
        .Where(c => c.StartsWith(text, StringComparison.OrdinalIgnoreCase))
        .Distinct()
        .OrderBy(c => c)
        .ToList();

    if (matches.Count == 0)
    {
      Reset();
      Console.Write("\x07");
      Console.Out.Flush();
      return null;
    }

    if (matches.Count == 1)
    {
      Reset();
      return $"{matches[0][text.Length..]} ";
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
      var prefix = GetLongestCommonPrefix(matches);
      if (prefix == text)
      {
        Console.Write("\x07");
        Console.Out.Flush();
        return null;
      }

      Reset();
      return prefix[text.Length..];
    }

    Console.WriteLine();
    Console.WriteLine(string.Join("  ", matches));
    Console.Write($"$ {text}");
    Console.Out.Flush();
    Reset();
    return null;
  }

  public void Reset()
  {
    _lastPrefix = "";
    _tabCount = 0;
  }

  private static string GetLongestCommonPrefix(List<string> matches)
  {
    var prefix = matches[0];

    foreach (var match in matches.Skip(1))
    {
      int len = 0;
      while (len < prefix.Length && len < match.Length && char.ToLowerInvariant(prefix[len]) == char.ToLowerInvariant(match[len]))
        len++;

      prefix = prefix[..len];

      if (prefix.Length == 0)
        break;
    }

    return prefix;
  }
}
