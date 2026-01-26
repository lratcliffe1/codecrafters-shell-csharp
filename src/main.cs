using src.Classes;
using src.Commands;
using src.Helpers;

class Program
{
  static async Task Main()
  {
    var autocomplete = new AutoCompletionEngine();
    var readline = new ReadLineEngine(autocomplete);

    var history = LoadHistoryFromHistFile();

    ShellContext shellContext = new ShellContext()
    {
      RawInput = "",
      Commands = [],
      WorkingDirectory = Directory.GetCurrentDirectory(),
      History = history,
      HistoryAppended = 0,
      HistoryLoaded = history.Count,
    };

    while (true)
    {
      string input = GetCommandFromUser(readline);

      if (input == "")
        continue;

      List<List<string>> formattedInput = Parcer.ParceUserInput(input);

      shellContext = ShellContextCreator.CreateShellContext(input, formattedInput, shellContext);

      if (shellContext.RawInput == "")
        continue;

      bool exit = false;

      foreach (var command in shellContext.Commands)
      {
        Stream? internalSource = null;

        switch (command.Name)
        {
          case "exit":
            internalSource = ExitCommand.Run(shellContext);
            break;
          case "echo":
            internalSource = EchoCommand.Run(command);
            break;
          case "pwd":
            internalSource = PwdCommand.Run(shellContext);
            break;
          case "cd":
            internalSource = CdCommand.Run(shellContext, command);
            break;
          case "type":
            internalSource = TypeCommand.Run(command);
            break;
          case "history":
            internalSource = HistoryCommand.Run(shellContext, command);
            break;
          default:
            await ExternalCommand.Run(shellContext, command);
            break;
        }

        if (internalSource != null)
        {
          // Capture the Task object
          var redirectionTask = ExternalCommand.ApplyRedirection(shellContext, command, internalSource);
          shellContext.OutputTasks.Add(redirectionTask);
        }

        if (command.Name == "exit")
        {
          exit = true;
          break;
        }
      }

      // After the foreach loop:
      if (shellContext.Processes.Any())
      {
        await shellContext.Processes.Last().WaitForExitAsync();
      }

      // THIS is where the magic happens for 2026 shells:
      // We wait for all background copy tasks to finish printing to the console.
      await Task.WhenAll(shellContext.OutputTasks);

      // Now it is safe to call OutputResult and restart the loop
      await OutputResult(shellContext);

      if (exit)
        return;
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

  private static async Task OutputResult(ShellContext shellContext)
  {
    // Ensure all process streams are finished
    foreach (var proc in shellContext.Processes)
    {
      // Drain any remaining redirected output
      await proc.WaitForExitAsync();
    }

    // Give a small yielding break for any background CopyToAsync tasks 
    // that are still pushing bits to the console buffer.
    await Task.Yield();

    if (shellContext.LastPipeReadStream != null)
    {
      await shellContext.LastPipeReadStream.DisposeAsync();
      shellContext.LastPipeReadStream = null;
    }

    shellContext.Processes.Clear();
  }
}

