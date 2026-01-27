using src.Classes;
using src.Helpers;

namespace src.Commands;

public static class TypeCommand
{
  static readonly List<string> builtInCommands = [.. CommandConstants.All];

  public static Stream Run(Command command) =>
      InternalCommand.CreateStream(async (writer) =>
      {
        string param = command.Args.Count > 0 ? command.Args[0] : string.Empty;
        if (builtInCommands.Contains(param))
          await writer.WriteLineAsync($"{param} is a shell builtin");
        else
        {
          string? path = FileExecuter.FindExecutablePath(param);
          await writer.WriteLineAsync(path != null ? $"{param} is {path}" : $"{param}: not found");
        }
      });
}
