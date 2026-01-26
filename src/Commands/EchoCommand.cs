using src.Classes;

namespace src.Commands;

public static class EchoCommand
{
  public static Stream Run(Command command) =>
      InternalCommand.CreateStream(async (writer) =>
      {
        await writer.WriteLineAsync(string.Join(" ", command.Args));
      });
}
