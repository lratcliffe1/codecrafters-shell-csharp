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
      AnonymousPipeClientStream? pipeClient = null;
      StreamWriter? writer = null;

      try
      {
        pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, clientHandle);
        writer = new StreamWriter(pipeClient);
        writer.AutoFlush = true;

        await logic(writer);
        await writer.FlushAsync();
      }
      catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
      {
        // Gracefully handle broken pipes (e.g., 'pwd | head -n 0')
      }
      finally
      {
        if (writer != null)
          await writer.DisposeAsync();

        if (pipeClient != null)
          await pipeClient.DisposeAsync();

        pipeServer.DisposeLocalCopyOfClientHandle();
      }
    });

    return pipeServer;
  }
}
