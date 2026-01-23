using src.Classes;
using src.Commands;
using src.Helpers;

class Program
{
  static async Task Main()
  {
    var autocomplete = new AutoCompletionEngine();
    var readline = new ReadLineEngine(autocomplete);

    ShellContext shellContext = new ShellContext()
    {
      RawInput = "",
      Command = "",
      Parameters = [],
      WorkingDirectory = Directory.GetCurrentDirectory(),
      History = LoadHistoryFromHistFile(),
    };

    while (true)
    {
      string input = GetCommandFromUser(readline);

      if (input == "")
        continue;

      List<string> formattedInput = Parcer.ParceUserInput(input);

      shellContext = CreateShellContext(input, formattedInput, shellContext);

      if (shellContext.RawInput == "")
        continue;

      switch (shellContext.Command)
      {
        case "exit":
          return;
        case "echo":
          EchoCommand.Run(shellContext);
          break;
        case "pwd":
          PwdCommand.Run(shellContext);
          break;
        case "cd":
          CdCommand.Run(shellContext);
          break;
        case "type":
          TypeCommand.Run(shellContext);
          break;
        case "history":
          HistoryCommand.Run(shellContext);
          break;
        default:
          await ExternalCommand.Run(shellContext);
          break;
      }

      await OutputResult(shellContext);
    }
  }

  static List<string> LoadHistoryFromHistFile()
  {
    var historyFilePath = Environment.GetEnvironmentVariable("HISTFILE");
    if (string.IsNullOrEmpty(historyFilePath))
      return [];

    string fileContent = File.ReadAllText(historyFilePath);

    return fileContent.Split("\n").SkipLast(1).ToList();
  }

  static string GetCommandFromUser(ReadLineEngine readLine)
  {
    var input = readLine.ReadLine("$ ");
    return input ?? "";
  }

  static ShellContext CreateShellContext(string input, List<string> formattedInput, ShellContext previousShellContext)
  {
    var operators = new[] {
      (Token: "2>>", IsError: true,  Type: OutputType.Append),
      (Token: "1>>", IsError: false, Type: OutputType.Append),
      (Token: ">>",  IsError: false, Type: OutputType.Append),
      (Token: "2>",  IsError: true,  Type: OutputType.Redirect),
      (Token: "1>",  IsError: false, Type: OutputType.Redirect),
      (Token: ">",   IsError: false, Type: OutputType.Redirect)
    };

    string? errorTarget = null;
    string? outputTarget = null;
    OutputType? outputType = null;

    foreach (var (token, isError, type) in operators)
    {
      int index = formattedInput.IndexOf(token);
      if (index != -1)
      {
        outputType = type;
        string target = formattedInput.Last();

        errorTarget = isError ? target : "Console";
        outputTarget = isError ? "Console" : target;

        formattedInput = formattedInput[..index];
        break;
      }
    }

    outputTarget ??= "Console";

    return new ShellContext
    {
      RawInput = input.ToLower(),
      Command = formattedInput[0].ToLower(),
      Parameters = formattedInput[1..],
      OutputTarget = outputTarget,
      ErrorTarget = errorTarget,
      WorkingDirectory = previousShellContext.WorkingDirectory,
      OutputType = outputType,
      History = previousShellContext.History.Append(input).ToList(),
    };
  }

  static async Task OutputResult(ShellContext shellInput)
  {
    if (shellInput.ErrorTarget != null)
    {
      switch (shellInput.ErrorTarget)
      {
        case "Console":
          if (!string.IsNullOrEmpty(shellInput.Error))
            Console.WriteLine(shellInput.Error);
          break;
        default:
          await FileExecuter.WriteToFile(shellInput.ErrorTarget, shellInput.Error, shellInput.OutputType);
          break;
      }
    }
    if (shellInput.OutputTarget != null)
    {
      switch (shellInput.OutputTarget)
      {
        case "Console":
          if (!string.IsNullOrEmpty(shellInput.Output))
            Console.WriteLine(shellInput.Output);
          break;
        default:
          await FileExecuter.WriteToFile(shellInput.OutputTarget, shellInput.Output, shellInput.OutputType);
          break;
      }
    }
  }
}

