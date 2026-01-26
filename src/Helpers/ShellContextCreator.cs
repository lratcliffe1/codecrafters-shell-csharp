using src.Classes;

namespace src.Helpers;

public static class ShellContextCreator
{
  private static readonly List<(string Token, bool IsError, OutputType Type)> operators = [
    ("2>>", true,  OutputType.Append),
    ("1>>", false, OutputType.Append),
    (">>",  false, OutputType.Append),
    ("2>",  true,  OutputType.Redirect),
    ("1>",  false, OutputType.Redirect),
    (">",   false, OutputType.Redirect)
  ];

  public static ShellContext CreateShellContext(string input, List<List<string>> formattedInputList, ShellContext previousShellContext)
  {
    var shellContext = new ShellContext()
    {
      RawInput = input.ToLower(),
      Commands = [],
      WorkingDirectory = previousShellContext.WorkingDirectory,
      History = previousShellContext.History.Append(input).ToList(),
      HistoryAppended = previousShellContext.HistoryAppended,
      HistoryLoaded = previousShellContext.HistoryLoaded,
    };

    for (var i = 0; i < formattedInputList.Count; i++)
    {
      List<string> formattedInput = formattedInputList[i];
      string? sterrTarget = null;
      string stdoutTarget = "Console";
      OutputType outputType = OutputType.None;

      foreach (var (token, isError, type) in operators)
      {
        int index = formattedInput.IndexOf(token);
        if (index != -1)
        {
          outputType = type;
          string target = formattedInput.Last();

          sterrTarget = isError ? target : "Console";
          stdoutTarget = isError ? "Console" : target;

          formattedInput = formattedInput[..index];
          break;
        }
      }

      var newCommand = new Command()
      {
        Name = formattedInput.First(),
        Args = formattedInput.Skip(1).ToList(),
        Index = i,
        StdoutTarget = stdoutTarget,
        SterrTarget = sterrTarget,
        OutputType = outputType,
      };

      shellContext.Commands.Add(newCommand);
    }

    return shellContext;
  }

}