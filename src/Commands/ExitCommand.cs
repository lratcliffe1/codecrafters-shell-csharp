using src.Classes;
using System.IO.Pipes;

namespace src.Commands;

public static class ExitCommand
{
  public static Stream Run(ShellContext shellContext)
  {
    // 1. Perform cleanup logic immediately
    SaveHistory(shellContext);

    // 2. Return an empty stream via the wrapper
    return InternalCommand.CreateStream(async (writer) =>
    {
      // Exit produces no output to stdout, so we do nothing here.
      // The wrapper handles opening and closing the pipe correctly.
      await Task.CompletedTask;
    });
  }

  private static void SaveHistory(ShellContext shellContext)
  {
    var historyFilePath = Environment.GetEnvironmentVariable("HISTFILE");
    if (string.IsNullOrEmpty(historyFilePath)) return;

    var newHistoryItems = shellContext.History.Skip(shellContext.HistoryLoaded);
    try { File.AppendAllLines(historyFilePath, newHistoryItems); }
    catch (IOException) { }
  }
}
