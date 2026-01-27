using src.Classes;
using src.Commands;
using src.Helpers;

class Program
{
  static async Task Main()
  {
    var history = LoadHistoryFromHistFile();

    var autocomplete = new AutoCompletionEngine();
    var readline = new ReadLineEngine(autocomplete, history.ToList());

    ShellContext shellContext = new()
    {
      RawInput = "",
      Commands = [],
      WorkingDirectory = Directory.GetCurrentDirectory(),
      History = history,
      HistoryAppended = history.Count,
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
          var redirectionTask = ExternalCommand.ApplyRedirection(shellContext, command, internalSource);
          shellContext.OutputTasks.Add(redirectionTask);
        }

        if (command.Name == "exit")
        {
          exit = true;
          break;
        }
      }

      if (shellContext.Processes.Any())
      {
        await shellContext.Processes.Last().WaitForExitAsync();
      }

      await Task.WhenAll(shellContext.OutputTasks);

      await OutputResult(shellContext);

      if (exit)
        return;
    }
  }

  static List<string> LoadHistoryFromHistFile()
  {
    string historyFilePath = Environment.GetEnvironmentVariable("HISTFILE")
      ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bash_history");

    if (!File.Exists(historyFilePath))
      return [];

    string[] fileContent = File.ReadAllLines(historyFilePath);

    return fileContent.ToList();
  }

  static string GetCommandFromUser(ReadLineEngine readLine)
  {
    var input = readLine.ReadLine("$ ");
    return input ?? "";
  }

  private static async Task OutputResult(ShellContext shellContext)
  {
    foreach (var proc in shellContext.Processes)
    {
      await proc.WaitForExitAsync();
    }

    await Task.Yield();

    if (shellContext.LastPipeReadStream != null)
    {
      await shellContext.LastPipeReadStream.DisposeAsync();
      shellContext.LastPipeReadStream = null;
    }

    shellContext.Processes.Clear();
  }
}

