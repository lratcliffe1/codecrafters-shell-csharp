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
      ShellInput shellInput = GetCommandFromUser();

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

      if (shellInput.Error != null)
      {
        switch (shellInput.ErrorTarget)
        {
          case "Console":
            Console.WriteLine(shellInput.Error);
            break;
          default:
            await FileExecuter.WriteToFile(shellInput);
            break;
        }
      }
      if (shellInput.Output != null)
      {
        switch (shellInput.OutputTarget)
        {
          case "Console":
            Console.WriteLine(shellInput.Output);
            break;
          default:
            await FileExecuter.WriteToFile(shellInput);
            break;
        }
      }
    }
}

static ShellInput GetCommandFromUser()
{
    Console.Write("$ ");
        
    string input = Console.ReadLine() ?? "";

    List<string> formattedInput = Parcer.ParceUserInput(input);

    string? outputTarget = "Console";

    int outputIndex = formattedInput.IndexOf(">");
    if (outputIndex != -1)
    {
      outputTarget = formattedInput.Last();
      formattedInput = formattedInput[..outputIndex];
    }
    outputIndex = formattedInput.IndexOf("1>");
    if (outputIndex != -1)
    {
      outputTarget = formattedInput.Last();
      formattedInput = formattedInput[..outputIndex];
    }

    return new ShellInput { 
      RawInput = input.ToLower(), 
      Command = formattedInput[0].ToLower(), 
      Parameters = formattedInput[1..],
      OutputTarget = outputTarget,
    };
  }
}
