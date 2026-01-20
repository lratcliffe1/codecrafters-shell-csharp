using src.Classes;

namespace src.Commands;

public static class PwdCommand
{
  public static void Run(ShellContext shellInput)
  {
    shellInput.Output = shellInput.WorkingDirectory;
  }
}