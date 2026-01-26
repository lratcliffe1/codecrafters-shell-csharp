using src.Classes;
using src.Helpers;
using System.IO.Pipes;

namespace src.Commands;

public static class TypeCommand
{
  static readonly List<string> builtInCommands = [.. CommandConstants.All];

  public static Stream Run(Command command)
  {
    // 1. Create the pipe
    var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
    var clientHandle = pipeServer.GetClientHandleAsString();

    // 2. Run the logic in a background task
    _ = Task.Run(async () =>
    {
      using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, clientHandle);
      using var writer = new StreamWriter(pipeClient);

      string param = command.Args[0];
      string output;

      if (builtInCommands.Contains(param))
      {
        output = $"{param} is a shell builtin";
      }
      else
      {
        string? executablePath = FileExecuter.FindExecutablePath(param);
        if (executablePath != null)
          output = $"{param} is {executablePath}";
        else
          output = $"{param}: not found";
      }

      await writer.WriteLineAsync(output);
      await writer.FlushAsync();
    });

    // 3. Return the read-side of the pipe to the shell
    return pipeServer;
  }
}
