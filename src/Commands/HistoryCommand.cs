using src.Classes;

namespace src.Commands;

public static class HistoryCommand
{
  private static int historyAppended = 0;

  public static void Run(ShellContext shellContext)
  {
    if (shellContext.Parameters.Count == 0)
      PrintFullHistory(shellContext);
    else if (shellContext.Parameters.Count == 1 && int.TryParse(shellContext.Parameters[0], out int commandLimit))
      PrintLimitedHistory(shellContext, commandLimit);
    else if (shellContext.Parameters.Count == 2 && shellContext.Parameters[0] == "-r")
      ReadHistoryFromFile(shellContext);
    else if (shellContext.Parameters.Count == 2 && shellContext.Parameters[0] == "-w")
      WriteHistoryToFile(shellContext);
    else if (shellContext.Parameters.Count == 2 && shellContext.Parameters[0] == "-a")
      AppendHistoryToFile(shellContext);
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

  private static void ReadHistoryFromFile(ShellContext shellContext)
  {
    string fileContent = File.ReadAllText(shellContext.Parameters[1]);

    foreach (var input in fileContent.Split("\n").SkipLast(1))
    {
      shellContext.History.Add(input);
    }
  }

  private static void WriteHistoryToFile(ShellContext shellContext)
  {
    string content = "";

    foreach (var input in shellContext.History)
    {
      content += $"{input}\n";
      historyAppended++;
    }

    File.WriteAllText(shellContext.Parameters[1], content);
  }

  private static void AppendHistoryToFile(ShellContext shellContext)
  {
    string content = "";

    foreach (var input in shellContext.History.Skip(historyAppended))
    {
      content += $"{input}\n";
    }

    File.AppendAllText(shellContext.Parameters[1], content);
    historyAppended = shellContext.History.Count;
  }
}