using src.Classes;
using System.IO.Pipes;

namespace src.Commands;

public static class HistoryCommand
{
  public static Stream Run(ShellContext shellContext, Command command) =>
      InternalCommand.CreateStream(async (writer) =>
      {
        var historySnapshot = shellContext.History.ToList();

        if (command.Args.Count == 0)
          await PrintFullHistory(historySnapshot, writer);
        else if (command.Args.Count == 1 && int.TryParse(command.Args[0], out int limit))
          await PrintLimitedHistory(historySnapshot, limit, writer);
        else if (command.Args.Count == 2 && command.Args[0] == "-r")
          ReadHistoryFromFile(shellContext, command.Args[1]);
        else if (command.Args.Count == 2 && command.Args[0] == "-w")
          WriteHistoryToFile(shellContext, command.Args[1]);
        else if (command.Args.Count == 2 && command.Args[0] == "-a")
          AppendHistoryToFile(shellContext, command.Args[1]);
      });

  private static async Task PrintFullHistory(List<string> history, StreamWriter writer)
  {
    for (int i = 0; i < history.Count; i++)
    {
      await writer.WriteLineAsync($"{i + 1,5}  {history[i]}");
    }
  }

  private static async Task PrintLimitedHistory(List<string> history, int limit, StreamWriter writer)
  {
    int totalCount = history.Count;
    int skip = Math.Max(0, totalCount - limit);

    for (int i = skip; i < totalCount; i++)
    {
      // {i + 1, 5}  provides the 5-character right-aligned column
      await writer.WriteLineAsync($"{i + 1,5}  {history[i]}");
    }
  }

  private static void ReadHistoryFromFile(ShellContext shellContext, string path)
  {
    if (!File.Exists(path)) return;
    var lines = File.ReadAllLines(path);
    foreach (var line in lines)
    {
      shellContext.History.Add(line);
    }
  }

  private static void WriteHistoryToFile(ShellContext shellContext, string path)
  {
    File.WriteAllLines(path, shellContext.History);
    shellContext.HistoryAppended = shellContext.History.Count;
  }

  private static void AppendHistoryToFile(ShellContext shellContext, string path)
  {
    var newLines = shellContext.History.Skip(shellContext.HistoryAppended);
    File.AppendAllLines(path, newLines);
    shellContext.HistoryAppended = shellContext.History.Count;
  }
}
