using src.Classes;

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
    string historyFilePath = Environment.GetEnvironmentVariable("HISTFILE")
      ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bash_history");

    if (string.IsNullOrEmpty(historyFilePath))
      return;

    var newHistoryItems = shellContext.History.Skip(shellContext.HistoryLoaded);
    try { File.AppendAllLines(historyFilePath, newHistoryItems); }
    catch (IOException) { }
  }
}
