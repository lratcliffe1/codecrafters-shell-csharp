using src.Classes;

namespace src.Commands;

public static class CdCommand
{
  public static void Run(ShellContext shellInput)
  {
    string home = Environment.GetEnvironmentVariable("HOME") ?? string.Empty;

    string[] parts = shellInput.Parameters[0].Split("/");

    if (parts.Length > 1 && parts[^1] == "")
    {
        parts = parts[0..^1];
    }

    string targetWorkingDirectory = shellInput.WorkingDirectory;

    foreach (string part in parts)
    {
        switch (part)
        {
            case "~":
                targetWorkingDirectory = home;
                break;
            case "":
                targetWorkingDirectory = "";
                break;
            case ".":
                targetWorkingDirectory = shellInput.WorkingDirectory;
                break;
            case "..":
                targetWorkingDirectory = targetWorkingDirectory.Substring(0, targetWorkingDirectory.LastIndexOf('/'));
                break;
            default:
                targetWorkingDirectory += "/" + part;
                break;
        }
    }

    if (!Directory.Exists(targetWorkingDirectory))
    {
        shellInput.Output = $"cd: {targetWorkingDirectory}: No such file or directory";
        return;
    }
    shellInput.OutputTarget = null;
    shellInput.WorkingDirectory = targetWorkingDirectory;
  }
}