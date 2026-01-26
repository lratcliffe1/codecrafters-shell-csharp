using src.Classes;
using System.IO.Pipes;

namespace src.Commands;

public static class HistoryCommand
{
  public static Stream Run(ShellContext shellContext, Command command)
  {
    // 1. Create the pipe
    var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
    string clientHandle = pipeServer.GetClientHandleAsString();

    // 2. Start the background producer
    _ = Task.Run(async () =>
    {
      try
      {
        // Use 'using' here to ensure local cleanup, but catch closure exceptions
        using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, clientHandle);
        using var writer = new StreamWriter(pipeClient);
        writer.AutoFlush = true;

        // Snapshot the history list to avoid 'Collection Modified' errors if user types fast
        var historySnapshot = shellContext.History.ToList();

        if (command.Args.Count == 0)
          await PrintFullHistory(historySnapshot, writer);
        else if (command.Args.Count == 1 && int.TryParse(command.Args[0], out int limit))
          await PrintLimitedHistory(historySnapshot, limit, writer);
        else if (command.Args.Count == 2 && command.Args[0] == "-r")
          ReadHistoryFromFile(shellContext, command.Args[1]);
        else if (command.Args.Count == 2 && command.Args[0] == "-r")
          ReadHistoryFromFile(shellContext, command.Args[1]);
        else if (command.Args.Count == 2 && command.Args[0] == "-w")
          WriteHistoryToFile(shellContext, command.Args[1]);
        else if (command.Args.Count == 2 && command.Args[0] == "-a")
          AppendHistoryToFile(shellContext, command.Args[1]);

        await writer.FlushAsync();
      }
      catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
      {
        // This prevents the EINVAL crash. If the redirection task finishes 
        // (e.g. history | head -n 1), this task will fail silently as expected.
      }
      finally
      {
        // Crucial for 2026: Dispose the local handle copy if not already done
        pipeServer.DisposeLocalCopyOfClientHandle();
      }
    });

    return pipeServer;
  }

  private static async Task PrintFullHistory(List<string> history, StreamWriter writer)
  {
    for (int i = 0; i < history.Count; i++)
    {
      // Fix for alignment issue found earlier
      await writer.WriteLineAsync($"{i + 1,5}  {history[i]}");
    }
  }

  private static async Task PrintLimitedHistory(List<string> history, int limit, StreamWriter writer)
  {
    int totalCount = history.Count;
    int skip = Math.Max(0, totalCount - limit);

    // We iterate using the original index to maintain correct numbering
    for (int i = skip; i < totalCount; i++)
    {
      // {i + 1, 5}  provides the 5-character right-aligned column
      // followed by the two spaces expected by the tester.
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
