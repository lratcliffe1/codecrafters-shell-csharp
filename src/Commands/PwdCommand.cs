using src.Classes;

namespace src.Commands;

public static class PwdCommand
{
  public static Stream Run(ShellContext shellInput) =>
      InternalCommand.CreateStream(async (writer) =>
      {
        await writer.WriteLineAsync(shellInput.WorkingDirectory);
      });
}
