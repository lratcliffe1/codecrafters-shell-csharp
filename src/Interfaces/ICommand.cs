using src.Classes;

namespace src.Interfaces;

public interface IComannd
{
  void Run(ShellInput shellInput, ref string workingDirectory);
}