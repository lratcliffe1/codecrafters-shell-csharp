using src.Classes;

namespace src.Commands;

public static class HistoryCommand
{
  public static void Run(ShellContext shellInput)
  {
    List<string> splitInput = shellInput.History.Last().Split(" ").ToList();

    int count = shellInput.History.Count;
    int limit = count;

    if (splitInput.Count > 1 && int.TryParse(splitInput[1], out int commandLimit))
    {
      limit = commandLimit;
    }

    int skip = count - limit;
    int index = 1 + skip;

    foreach (var input in shellInput.History.Skip(skip))
    {
      Console.WriteLine($"{index++} {input}");
    }
  }
}