namespace src.Helpers;

using src.Classes;

public static class Parcer
{
  private static readonly HashSet<char> BackslashInDoubleQuotesEscapes = new(['"', '\\', '$', '`', '\n']);

  /// <summary>
  /// Single quotes disable all special characters enclosed within them.
  /// Double quotes disable all special characters enclosed within them except $ and \.
  /// Backslash outside of quotes escapes the next character.
  /// Backslash inside single quotes has no effect.
  /// Backslash inside double quotes escapes ", \, $, `, and newline.
  /// </summary>
  public static List<string> ParceUserInput(string input)
  {
    List<string> output = [];
    var current = new System.Text.StringBuilder();

    ParcingContext ctx = new()
    {
      WithinSingleQuotes = false,
      WithinDoubleQuotes = false,
      EscapeNextCharacter = false,
    };

    foreach (char c in input)
    {
      if (ctx.WithinSingleQuotes)
      {
        ParseWithinSingleQuotes(ctx, current, c);
      }
      else if (ctx.WithinDoubleQuotes)
      {
        ParseWithinDoubleQuotes(ctx, current, c);
      }
      else
      {
        ParseOutsideOfQuotes(ctx, output, current, c);
      }
    }

    // If input ends right after a backslash outside quotes, treat it literally.
    if (ctx.EscapeNextCharacter)
    {
      current.Append('\\');
      ctx.EscapeNextCharacter = false;
    }

    if (current.Length > 0)
      output.Add(current.ToString());

    return output;
  }

  private static void ParseWithinSingleQuotes(ParcingContext ctx, System.Text.StringBuilder current, char c)
  {
    // No escaping in single quotes (per your comment)
    if (c == '\'')
    {
      ctx.WithinSingleQuotes = false;
      return;
    }

    current.Append(c);
  }

  private static void ParseWithinDoubleQuotes(ParcingContext ctx, System.Text.StringBuilder current, char c)
  {
    if (ctx.EscapeNextCharacter)
    {
      ctx.EscapeNextCharacter = false;

      // Backslash-newline inside double quotes = line continuation: remove both.
      if (c == '\n')
        return;

      // Only these are actually escaped; everything else keeps the backslash.
      if (BackslashInDoubleQuotesEscapes.Contains(c))
      {
        current.Append(c);
      }
      else
      {
        current.Append('\\');
        current.Append(c);
      }

      return;
    }

    if (c == '"')
    {
      ctx.WithinDoubleQuotes = false;
      return;
    }

    if (c == '\\')
    {
      ctx.EscapeNextCharacter = true;
      return;
    }

    current.Append(c);
  }

  private static void ParseOutsideOfQuotes(
    ParcingContext ctx,
    List<string> output,
    System.Text.StringBuilder current,
    char c)
  {
    if (ctx.EscapeNextCharacter)
    {
      ctx.EscapeNextCharacter = false;
      current.Append(c);
      return;
    }

    if (c == '\\')
    {
      ctx.EscapeNextCharacter = true;
      return;
    }

    if (c == '"')
    {
      ctx.WithinDoubleQuotes = true;
      return;
    }

    if (c == '\'')
    {
      ctx.WithinSingleQuotes = true;
      return;
    }

    if (char.IsWhiteSpace(c))
    {
      if (current.Length > 0)
      {
        output.Add(current.ToString());
        current.Clear();
      }
      return;
    }

    current.Append(c);
  }
}
