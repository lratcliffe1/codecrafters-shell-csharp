using src.Helpers;

public sealed class AutoCompletionEngine
{
  public char[] Separators { get; set; } = "abcdefghijklmnopqrstuvwxyz_".ToArray();

  private readonly string[] _commands = CommandConstants.All;
  private string _lastPrefix = "";
  private int _tabCount = 0;

  public string? Complete(string text)
  {
    var context = CreateContext(text);

    var matches = BuildCandidates(context)
      .Where(c => c.Text.StartsWith(context.Token, StringComparison.OrdinalIgnoreCase))
      .DistinctBy(c => c.Text, StringComparer.OrdinalIgnoreCase)
      .OrderBy(c => c.Text, StringComparer.OrdinalIgnoreCase)
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
      var suffix = matches[0].Text[context.Token.Length..];
      return matches[0].IsDirectory ? suffix : $"{suffix} ";
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
      var prefix = GetLongestCommonPrefix(matches.Select(match => match.Text).ToList());
      if (prefix == context.Token)
      {
        Console.Write("\x07");
        Console.Out.Flush();
        return null;
      }

      Reset();
      return prefix[context.Token.Length..];
    }

    Console.WriteLine();
    Console.WriteLine(string.Join("  ", matches.Select(x => x.Text)));
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

  private static CompletionContext CreateContext(string text)
  {
    var tokenStartIndex = GetTokenStartIndex(text);
    var token = tokenStartIndex >= text.Length ? "" : text[tokenStartIndex..];
    var isFirstToken = string.IsNullOrWhiteSpace(text[..tokenStartIndex]);

    return new CompletionContext(Token: token, IsFirstToken: isFirstToken);
  }

  private static int GetTokenStartIndex(string text)
  {
    for (var i = text.Length - 1; i >= 0; i--)
    {
      if (char.IsWhiteSpace(text[i]))
        return i + 1;
    }

    return 0;
  }

  private IEnumerable<CompletionCandidate> BuildCandidates(CompletionContext context)
  {
    if (context.IsFirstToken && !ContainsPathSeparator(context.Token))
    {
      return _commands
        .Concat(FileExecuter.FindExecutablesAtPath())
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Select(value => new CompletionCandidate(value!, IsDirectory: false));
    }

    return BuildFileCandidates(context.Token);
  }

  private static IEnumerable<CompletionCandidate> BuildFileCandidates(string token)
  {
    var hasPathSeparator = ContainsPathSeparator(token);
    var directoryToken = hasPathSeparator ? Path.GetDirectoryName(token) ?? "" : "";
    var searchDirectory = ResolveSearchDirectory(directoryToken);

    if (searchDirectory == null || !Directory.Exists(searchDirectory))
      return [];

    var displayPrefix = GetDisplayPrefix(token);

    var directories = Directory.GetDirectories(searchDirectory)
      .Select(Path.GetFileName)
      .Where(name => !string.IsNullOrWhiteSpace(name))
      .Select(name => new CompletionCandidate($"{displayPrefix}{name!}/", IsDirectory: true));

    var files = Directory.GetFiles(searchDirectory)
      .Select(Path.GetFileName)
      .Where(name => !string.IsNullOrWhiteSpace(name))
      .Select(name => new CompletionCandidate($"{displayPrefix}{name!}", IsDirectory: false));

    return directories.Concat(files);
  }

  private static bool ContainsPathSeparator(string value)
  {
    return value.Contains(Path.DirectorySeparatorChar) || value.Contains(Path.AltDirectorySeparatorChar);
  }

  private static string? ResolveSearchDirectory(string directoryToken)
  {
    if (string.IsNullOrWhiteSpace(directoryToken))
      return Directory.GetCurrentDirectory();

    try
    {
      return Path.IsPathRooted(directoryToken)
        ? directoryToken
        : Path.GetFullPath(directoryToken, Directory.GetCurrentDirectory());
    }
    catch
    {
      return null;
    }
  }

  private static string GetDisplayPrefix(string token)
  {
    var separatorIndex = token.LastIndexOf(Path.DirectorySeparatorChar);
    var altSeparatorIndex = token.LastIndexOf(Path.AltDirectorySeparatorChar);
    var lastSeparatorIndex = Math.Max(separatorIndex, altSeparatorIndex);

    if (lastSeparatorIndex < 0)
      return "";

    return token[..(lastSeparatorIndex + 1)];
  }

  private readonly record struct CompletionContext(string Token, bool IsFirstToken);
  private readonly record struct CompletionCandidate(string Text, bool IsDirectory);
}
