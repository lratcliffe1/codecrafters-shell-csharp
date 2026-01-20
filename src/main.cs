using src.Classes;
using src.Commands;
using src.Helpers;

class Program
{
  static async Task Main()
  {        
    string workingDirectory = Directory.GetCurrentDirectory();

    while (true)
    {
      ShellContext shellInput = GetCommandFromUser();

      if (shellInput.RawInput == "")
        continue;

      switch (shellInput.Command)
      {
        case "exit":
          return;
        case "echo":
          EchoCommand.Run(shellInput, ref workingDirectory);
          break;
        case "pwd":
          PwdCommand.Run(shellInput, ref workingDirectory);
          break;
        case "cd":
          CdCommand.Run(shellInput, ref workingDirectory);
          break;
        case "type":
          TypeCommand.Run(shellInput, ref workingDirectory);
          break;
        default:
          await ExternalCommand.Run(shellInput);
          break;
      }

      if (shellInput.ErrorTarget != null)
      {
        switch (shellInput.ErrorTarget)
        {
          case "Console":
            if (!string.IsNullOrEmpty(shellInput.Error))
              Console.WriteLine(shellInput.Error);
            break;
          default:
            await FileExecuter.WriteToFile(shellInput.ErrorTarget, shellInput.Error);
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
            await FileExecuter.WriteToFile(shellInput.OutputTarget, shellInput.Output);
            break;
        }
      }
    }
}

static ShellContext GetCommandFromUser()
{
    Console.Write("$ ");
        
    string input = Console.ReadLine() ?? "";

    List<string> formattedInput = Parcer.ParceUserInput(input);

    string? errorTarget = null;
    string? outputTarget = null;

    var errorIndex = formattedInput.IndexOf("2>");
    int outputIndex1 = formattedInput.IndexOf("1>");
    int outputIndex = formattedInput.IndexOf(">");
    
    if (errorIndex != -1)
    {
      outputTarget = "Console";
      errorTarget = formattedInput.Last();
      formattedInput = formattedInput[..errorIndex];
    } 
    else if (outputIndex1 != -1)
    {
      errorTarget = "Console";
      outputTarget = formattedInput.Last();
      formattedInput = formattedInput[..outputIndex1];
    }
    else if (outputIndex != -1)
    {
      errorTarget = "Console";
      outputTarget = formattedInput.Last();
      formattedInput = formattedInput[..outputIndex];
    }

    if (errorTarget == null && outputTarget == null)
      outputTarget = "Console";

    return new ShellContext { 
      RawInput = input.ToLower(), 
      Command = formattedInput[0].ToLower(), 
      Parameters = formattedInput[1..],
      OutputTarget = outputTarget,
      ErrorTarget = errorTarget,
    };
  }
}
