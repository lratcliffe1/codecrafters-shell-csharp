using System.Diagnostics;
using System.Text;
using src.Classes;
using src.Helpers;

namespace src.Commands;

public static class ExternalCommand
{
  public static async Task Run(ShellInput shellInput)
  {
    string? executablePath = FileExecuter.FindExecutablePath(shellInput.Command);

    if (executablePath != null)
    {
      var outputBuilder = new StringBuilder();
      var errorBuilder = new StringBuilder();

      using var process = new Process();
      process.StartInfo = new ProcessStartInfo
      {
        FileName = shellInput.Command,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };

      foreach (var param in shellInput.Parameters)
      {
          process.StartInfo.ArgumentList.Add(param);
      }

      process.OutputDataReceived += (s, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
      process.ErrorDataReceived += (s, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

      process.Start();

      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      await process.WaitForExitAsync();

      string finalError = errorBuilder.ToString().Trim();
      string finalOutput = outputBuilder.ToString().Trim();

      shellInput.Output = finalOutput;
      
      if (!string.IsNullOrEmpty(finalError))
      {
        shellInput.Error = finalError;
        shellInput.ErrorTarget = "Console";
      }
    }
    else
    {
      shellInput.Output = $"{shellInput.RawInput}: command not found";
      shellInput.OutputTarget = "Console";
    }
  }
}