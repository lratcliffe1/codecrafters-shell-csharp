using src.Classes;

namespace src.Commands;

public static class JobsCommand
{
  public static Stream Run(Command _) => InternalCommand.CreateStream((_) => Task.CompletedTask);
}
