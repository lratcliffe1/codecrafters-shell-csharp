using src.Classes;
using System.IO.Pipes;

namespace src.Commands;

public static class CdCommand
{
  public static Stream Run(ShellContext shellInput, Command command)
  {
    var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
    var clientHandle = pipeServer.GetClientHandleAsString();

    _ = Task.Run(async () =>
    {
      using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, clientHandle);
      using var writer = new StreamWriter(pipeClient);

      if (command.Args.Count == 0) return;

      string home = Environment.GetEnvironmentVariable("HOME") ?? string.Empty;
      string pathArg = command.Args[0];

      // Handle absolute vs relative start
      string targetWorkingDirectory = pathArg.StartsWith("/")
              ? ""
              : shellInput.WorkingDirectory;

      string[] parts = pathArg.Split('/', StringSplitOptions.RemoveEmptyEntries);

      // Manual path traversal logic
      if (pathArg.StartsWith("~"))
      {
        targetWorkingDirectory = home;
        parts = parts.Skip(1).ToArray();
      }

      foreach (string part in parts)
      {
        if (part == "..")
        {
          int lastIndex = targetWorkingDirectory.LastIndexOf('/');
          targetWorkingDirectory = lastIndex > 0 ? targetWorkingDirectory[..lastIndex] : "/";
        }
        else if (part != ".")
        {
          targetWorkingDirectory = targetWorkingDirectory.TrimEnd('/') + "/" + part;
        }
      }

      // Validation
      if (!Directory.Exists(targetWorkingDirectory))
      {
        // Errors in shells usually go to Stderr, but for your internal 
        // stream consistency, we write the error to the stream.
        await writer.WriteLineAsync($"cd: {pathArg}: No such file or directory");
      }
      else
      {
        shellInput.WorkingDirectory = targetWorkingDirectory;
      }

      await writer.FlushAsync();
    });

    return pipeServer;
  }
}
