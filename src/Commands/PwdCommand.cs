using src.Classes;

namespace src.Commands;

public static class PwdCommand
{
  public static void Run(ShellInput shellInput, ref string workingDirectory)
  {
    shellInput.Output = workingDirectory;
    shellInput.OutputTarget = "Console";
  }
}