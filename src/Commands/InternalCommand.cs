namespace src.Commands;

public static class InternalCommand
{
  public static Stream CreateStream(Func<StreamWriter, Task> logic)
  {
    var stream = new MemoryStream();
    using var writer = new StreamWriter(stream, leaveOpen: true)
    {
      AutoFlush = true
    };

    try
    {
      logic(writer).GetAwaiter().GetResult();
      writer.Flush();
    }
    catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
    {
      // Gracefully handle cases where output is intentionally discarded.
    }

    stream.Position = 0;
    return stream;
  }
}
