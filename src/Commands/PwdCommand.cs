using src.Classes;
using System.IO.Pipes;

namespace src.Commands;

public static class PwdCommand
{
  public static Stream Run(ShellContext shellInput)
  {
    // 1. Create the pipe for inter-process (or inter-command) communication
    var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
    var clientHandle = pipeServer.GetClientHandleAsString();

    // 2. Run the logic in a background task to populate the stream
    _ = Task.Run(async () =>
    {
      using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, clientHandle);
      using var writer = new StreamWriter(pipeClient);

      // Write the current working directory to the pipe
      await writer.WriteLineAsync(shellInput.WorkingDirectory);
      await writer.FlushAsync();
      // Disposing pipeClient here signals EOF to the reader
    });

    // 3. Return the read-side of the pipe to the shell's ApplyRedirection logic
    return pipeServer;
  }
}
