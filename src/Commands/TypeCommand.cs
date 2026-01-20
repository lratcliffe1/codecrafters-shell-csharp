using src.Classes;
using src.Helpers;

namespace src.Commands;

public static class TypeCommand
{
  static readonly List<string> builtInCommands = ["exit", "echo", "type", "pwd", "cd"];

  public static void Run(ShellContext shellInput)
  {
    if (builtInCommands.Contains(shellInput.Parameters[0]))
    {
      shellInput.Output = $"{shellInput.Parameters[0]} is a shell builtin";
      return;
    }
    
    string? executablePath = FileExecuter.FindExecutablePath(shellInput.Parameters[0]);

    if (executablePath != null)
      shellInput.Output = $"{shellInput.Parameters[0]} is {executablePath}";
    else
      shellInput.Output = $"{shellInput.Parameters[0]}: not found";
  }
}