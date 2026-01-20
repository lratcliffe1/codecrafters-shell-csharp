using src.Classes;

namespace src.Commands;

public static class EchoCommand
{
  public static void Run(ShellContext shellInput, ref string workingDirectory)
  {
    shellInput.Output = string.Join(" ", shellInput.Parameters);
  }
}