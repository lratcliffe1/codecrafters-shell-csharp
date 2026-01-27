using src.Classes;
using System.IO.Pipes;

namespace src.Commands;

public static class ExitCommand
{
  public static Stream Run(ShellContext shellContext)
  {
    SaveHistory(shellContext);

    return InternalCommand.CreateStream(async (writer) =>
    {
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
