using src.Classes;
using System.IO.Pipes;

namespace src.Commands;

public static class ExitCommand
{
  public static Stream Run(ShellContext shellContext)
  {
    // 1. Perform the "Exit" cleanup logic (Saving History)
    SaveHistory(shellContext);

    // 2. Create an empty pipe to satisfy the pipeline architecture
    var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
    var clientHandle = pipeServer.GetClientHandleAsString();

    _ = Task.Run(() =>
    {
      // Simply close the client side immediately as exit produces no output
      using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, clientHandle);
    });

    return pipeServer;
  }

  private static void SaveHistory(ShellContext shellContext)
  {
    var historyFilePath = Environment.GetEnvironmentVariable("HISTFILE");
    if (string.IsNullOrEmpty(historyFilePath))
      return;

    // Use AppendAllLines for better memory management and performance in 2026
    var newHistoryItems = shellContext.History.Skip(shellContext.HistoryLoaded);

    try
    {
      File.AppendAllLines(historyFilePath, newHistoryItems);
    }
    catch (IOException)
    {
      // Standard shell behavior: fail silently or log to stderr if history cannot be saved
    }
  }
}
