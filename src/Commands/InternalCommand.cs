using System.IO.Pipes;

namespace src.Commands;

public static class InternalCommand
{
  /// <summary>
  /// Wraps internal logic in a 2026-compliant Anonymous Pipe stream.
  /// Handles broken pipes, handle disposal, and prevents EINVAL crashes.
  /// </summary>
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

        // CRITICAL: Prevent EINVAL by disposing handle local copy
        pipeServer.DisposeLocalCopyOfClientHandle();
      }
    });

    return pipeServer;
  }
}
