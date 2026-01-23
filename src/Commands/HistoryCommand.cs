using src.Classes;

namespace src.Commands;

public static class HistoryCommand
{
  public static void Run(ShellContext shellContext)
  {
    if (shellContext.Parameters.Count == 0)
      PrintFullHistory(shellContext);
    else if (shellContext.Parameters.Count == 1 && int.TryParse(shellContext.Parameters[0], out int commandLimit))
      PrintLimitedHistory(shellContext, commandLimit);
    else if (shellContext.Parameters.Count == 2 && shellContext.Parameters[0] == "-r")
      PrintHistoryFromFile(shellContext);
  }

  private static void PrintFullHistory(ShellContext shellContext)
  {
    int index = 1;

    foreach (var input in shellContext.History)
    {
      Console.WriteLine($"{index++} {input}");
    }
  }

  private static void PrintLimitedHistory(ShellContext shellContext, int limit)
  {
    int count = shellContext.History.Count;

    int skip = count - limit;
    int index = 1 + skip;

    foreach (var input in shellContext.History.Skip(skip))
    {
      Console.WriteLine($"{index++} {input}");
    }
  }

  private static void PrintHistoryFromFile(ShellContext shellContext)
  {
    string fileContent = File.ReadAllText(shellContext.Parameters[1]);

    foreach (var input in fileContent.Split("\n").SkipLast(1))
    {
      shellContext.History.Add(input);
    }
  }
}