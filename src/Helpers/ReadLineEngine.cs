namespace src.Helpers;

using System.Text;

public sealed class ReadLineEngine
{
  private readonly AutoCompletionEngine _autocomplete;
  private readonly List<string> _history = [];
  private int _historyIndex; // points to "current" slot; _history.Count == blank line after last

  public ReadLineEngine(AutoCompletionEngine autocomplete)
  {
    _autocomplete = autocomplete;
    _historyIndex = 0;
  }

  public string ReadLine(string prompt)
  {
    var buffer = new StringBuilder();

    _historyIndex = _history.Count;

    Console.Write(prompt);
    Console.Out.Flush();

    while (true)
    {
      var key = KeyReader.Read();

      switch (key.Kind)
      {
        case KeyKind.Char:
          buffer.Append(key.Char);
          Redraw(prompt, buffer.ToString());
          break;

        case KeyKind.Backspace:
          if (buffer.Length > 0)
          {
            buffer.Remove(buffer.Length - 1, 1);
            Redraw(prompt, buffer.ToString());
          }
          break;

        case KeyKind.Tab:
          {
            var completion = _autocomplete.Complete(buffer.ToString());
            if (!string.IsNullOrEmpty(completion))
            {
              buffer.Append(completion);
              Redraw(prompt, buffer.ToString());
            }
            break;
          }

        case KeyKind.Up:
          {
            var prev = HistoryPrevious();
            buffer.Clear();
            buffer.Append(prev);
            Redraw(prompt, buffer.ToString());
            break;
          }

        case KeyKind.Down:
          {
            var next = HistoryNext();
            buffer.Clear();
            buffer.Append(next);
            Redraw(prompt, buffer.ToString());
            break;
          }

        case KeyKind.Enter:
          {
            Console.WriteLine();
            var line = buffer.ToString();

            if (!string.IsNullOrWhiteSpace(line))
              _history.Add(line);

            _autocomplete.Reset();
            return line;
          }

        default:
          break;
      }
    }
  }

  private string HistoryPrevious()
  {
    if (_history.Count == 0)
      return "";

    if (_historyIndex <= 0)
    {
      _historyIndex = 0;
      return _history[0];
    }

    _historyIndex--;
    return _history[_historyIndex];
  }

  private string HistoryNext()
  {
    if (_history.Count == 0)
      return "";

    if (_historyIndex >= _history.Count - 1)
    {
      _historyIndex = _history.Count;
      return "";
    }

    _historyIndex++;
    return _history[_historyIndex];
  }

  private static void Redraw(string prompt, string buffer)
  {
    Console.Write("\r");
    Console.Write(prompt);
    Console.Write(buffer);
    Console.Write("\x1b[K"); // ANSI: clear to end of line
    Console.Out.Flush();
  }
}

public enum KeyKind
{
  Char,
  Enter,
  Backspace,
  Tab,
  Up,
  Down,
  Left,
  Right,
  Unknown
}

public readonly record struct KeyPress(KeyKind Kind, char Char = '\0');

public static class KeyReader
{
  public static KeyPress Read()
  {
    if (!Console.IsInputRedirected)
    {
      var k = Console.ReadKey(intercept: true);
      return k.Key switch
      {
        ConsoleKey.Enter => new KeyPress(KeyKind.Enter),
        ConsoleKey.Backspace => new KeyPress(KeyKind.Backspace),
        ConsoleKey.Tab => new KeyPress(KeyKind.Tab),
        ConsoleKey.UpArrow => new KeyPress(KeyKind.Up),
        ConsoleKey.DownArrow => new KeyPress(KeyKind.Down),
        ConsoleKey.LeftArrow => new KeyPress(KeyKind.Left),
        ConsoleKey.RightArrow => new KeyPress(KeyKind.Right),
        _ when !char.IsControl(k.KeyChar) => new KeyPress(KeyKind.Char, k.KeyChar),
        _ => new KeyPress(KeyKind.Unknown)
      };
    }

    // Redirected input (tester): read chars and parse ANSI escape sequences.
    int ch = Console.In.Read();
    if (ch == -1) return new KeyPress(KeyKind.Unknown);

    char c = (char)ch;

    // Some harnesses send \n only; some send \r\n
    if (c == '\n') return new KeyPress(KeyKind.Enter);
    if (c == '\r')
    {
      if (Console.In.Peek() == '\n') Console.In.Read();
      return new KeyPress(KeyKind.Enter);
    }

    if (c == '\t') return new KeyPress(KeyKind.Tab);

    // Backspace can be '\b' or DEL (127)
    if (c == '\b' || c == (char)127) return new KeyPress(KeyKind.Backspace);

    // ANSI arrow keys: ESC [ A/B/C/D
    if (c == '\x1b') // ESC
    {
      if (Console.In.Peek() == '[')
      {
        Console.In.Read(); // consume '['
        int code = Console.In.Read();
        return code switch
        {
          (int)'A' => new KeyPress(KeyKind.Up),
          (int)'B' => new KeyPress(KeyKind.Down),
          (int)'C' => new KeyPress(KeyKind.Right),
          (int)'D' => new KeyPress(KeyKind.Left),
          _ => new KeyPress(KeyKind.Unknown)
        };
      }

      return new KeyPress(KeyKind.Unknown);
    }

    if (!char.IsControl(c))
      return new KeyPress(KeyKind.Char, c);

    return new KeyPress(KeyKind.Unknown);
  }
}
