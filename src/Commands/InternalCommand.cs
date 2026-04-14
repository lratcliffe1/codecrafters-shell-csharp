using System.IO.Pipes;

namespace src.Commands;

public static class InternalCommand
{
  public static Stream CreateStream(Func<StreamWriter, Task> logic)
  {
    var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.None);
    string clientHandle = pipeServer.GetClientHandleAsString();

    _ = Task.Run(async () =>
    {
      try
      {
        await using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, clientHandle);
        await using var writer = new StreamWriter(pipeClient)
        {
          AutoFlush = true
        };

        await logic(writer);
        await writer.FlushAsync();
      }
      catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
      {
        // Gracefully handle broken pipes (e.g., 'pwd | head -n 0')
      }
      finally
      {
        pipeServer.DisposeLocalCopyOfClientHandle();
      }
    });

    return pipeServer;
  }
}
