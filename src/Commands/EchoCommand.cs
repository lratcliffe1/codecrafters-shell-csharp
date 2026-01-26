using System.IO.Pipes;
using src.Classes;

namespace src.Commands;

public static class EchoCommand
{
  public static Stream Run(Command command)
  {
    // 1. Create a pipe to hold the output of the echo command
    var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

    // 2. Get the write-side handle
    var clientHandle = pipeServer.GetClientHandleAsString();

    // 3. Start a task to write the data so we don't block the shell
    _ = Task.Run(async () =>
    {
      using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, clientHandle);
      using var writer = new StreamWriter(pipeClient);

      string output = string.Join(" ", command.Args);
      await writer.WriteLineAsync(output);
      await writer.FlushAsync();
      // Closing the writer/pipeClient signals 'EOF' to the reader
    });

    // 4. Return the read side to the shell loop
    return pipeServer;
  }
}