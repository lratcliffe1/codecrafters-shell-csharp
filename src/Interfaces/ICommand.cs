using src.Classes;

namespace src.Interfaces;

public interface IComannd
{
  void Run(ShellContext shellInput, ref string workingDirectory);
}