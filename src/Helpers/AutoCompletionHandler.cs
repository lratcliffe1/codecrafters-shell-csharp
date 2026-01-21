using src.Helpers;

class AutoCompletionHandler : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = [' ', '.', '/'];
    
    private readonly string[] _commands = ["exit", "echo", "pwd", "cd", "type"];
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
            return [$"{matches[0]} "];
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
            Console.Write("\x07");
            return null!;
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
}
