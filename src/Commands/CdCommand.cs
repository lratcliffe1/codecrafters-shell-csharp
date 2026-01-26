using src.Classes;

namespace src.Commands;

public static class CdCommand
{
  public static Stream Run(ShellContext shellInput, Command command) =>
      InternalCommand.CreateStream(async (writer) =>
      {
        if (command.Args.Count == 0) return;

        string home = Environment.GetEnvironmentVariable("HOME") ?? string.Empty;
        string pathArg = command.Args[0];
        string target = pathArg.StartsWith("/") ? "" : shellInput.WorkingDirectory;

        if (pathArg.StartsWith("~")) { target = home; pathArg = pathArg[1..]; }

        var parts = pathArg.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
          if (part == "..") target = target.Contains('/') ? target[..target.LastIndexOf('/')] : "/";
          else if (part != ".") target = target.TrimEnd('/') + "/" + part;
        }

        if (target == "") target = "/";

        if (!Directory.Exists(target))
          await writer.WriteLineAsync($"cd: {command.Args[0]}: No such file or directory");
        else
          shellInput.WorkingDirectory = target;
      });
}
