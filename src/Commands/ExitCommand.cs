using src.Classes;

namespace src.Commands;

public static class ExitCommand
{
  public static void Run(ShellContext shellContext)
  {
    var historyFilePath = Environment.GetEnvironmentVariable("HISTFILE");
    if (string.IsNullOrEmpty(historyFilePath))
      return;

    string content = "";

    foreach (var input in shellContext.History)
    {
      content += $"{input}\n";
    }

    File.AppendAllText(historyFilePath, content);
  }
}