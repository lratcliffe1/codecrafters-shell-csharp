using src.Classes;
using src.Commands;
using src.Helpers;

class Program
{
  static async Task Main()
  {
    ShellContext? shellContext = null;

    while (true)
    {
      string input = GetCommandFromUser();

      List<string> formattedInput = Parcer.ParceUserInput(input);

      shellContext = CreateShellContext(input, formattedInput, shellContext?.WorkingDirectory);

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
        default:
          await ExternalCommand.Run(shellContext);
          break;
      }

      await OutputResult(shellContext);
    }
  }

  static string GetCommandFromUser()
  {
    Console.Write("$ ");
        
    return Console.ReadLine() ?? "";
  }

  static ShellContext CreateShellContext(string input, List<string> formattedInput, string? workingDirectory)
  {
    workingDirectory ??= Directory.GetCurrentDirectory();

    string? errorTarget = null;
    string? outputTarget = null;
    OutputType? outputType = null;

    var appendIndex1 = formattedInput.IndexOf("1>>");
    var appendIndex = formattedInput.IndexOf(">>");
    var errorIndex = formattedInput.IndexOf("2>");
    int outputIndex1 = formattedInput.IndexOf("1>");
    int outputIndex = formattedInput.IndexOf(">");
    
    if (appendIndex1 != -1)
    {
      errorTarget = "Console";
      outputTarget = formattedInput.Last();
      formattedInput = formattedInput[..appendIndex1];
      outputType = OutputType.Append;
    }
    else if (appendIndex != -1)
    {
      errorTarget = "Console";
      outputTarget = formattedInput.Last();
      formattedInput = formattedInput[..appendIndex];
      outputType = OutputType.Append;
    }
    else if (errorIndex != -1)
    {
      outputTarget = "Console";
      errorTarget = formattedInput.Last();
      formattedInput = formattedInput[..errorIndex];
      outputType = OutputType.Redirect;
    } 
    else if (outputIndex1 != -1)
    {
      errorTarget = "Console";
      outputTarget = formattedInput.Last();
      formattedInput = formattedInput[..outputIndex1];
      outputType = OutputType.Redirect;
    }
    else if (outputIndex != -1)
    {
      errorTarget = "Console";
      outputTarget = formattedInput.Last();
      formattedInput = formattedInput[..outputIndex];
      outputType = OutputType.Redirect;
    }

    if (errorTarget == null && outputTarget == null)
      outputTarget = "Console";

    return new ShellContext { 
      RawInput = input.ToLower(), 
      Command = formattedInput[0].ToLower(), 
      Parameters = formattedInput[1..],
      OutputTarget = outputTarget,
      ErrorTarget = errorTarget,
      WorkingDirectory = workingDirectory,
      OutputType = outputType,
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

