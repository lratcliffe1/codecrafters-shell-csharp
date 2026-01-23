using src.Classes;

namespace src.Commands;

public static class HistoryCommand
{
  public static void Run(ShellContext shellInput)
  {
    int index = 1;

    foreach (var input in shellInput.History)
    {
      Console.WriteLine($"{index} {input}");
      index++;
    }
  }
}