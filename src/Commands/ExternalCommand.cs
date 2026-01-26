using System.Diagnostics;
using System.IO.Pipes;
using src.Classes;

namespace src.Commands;

public class ExternalCommand
{
  public static async Task Run(ShellContext shellInput, Command command)
  {
    var proc = new Process();
    proc.StartInfo = new ProcessStartInfo(command.Name)
    {
      RedirectStandardInput = true,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false
    };

    foreach (var arg in command.Args)
    {
      proc.StartInfo.ArgumentList.Add(arg);
    }

    try
    {
      proc.Start();
      shellInput.Processes.Add(proc);

      // LINK INPUT: Pull from previous pipe
      if (shellInput.LastPipeReadStream != null)
      {
        _ = CopyStreamAsync(shellInput.LastPipeReadStream, proc.StandardInput.BaseStream);
      }

      // Apply redirection logic if start is successful
      var outputTask = ApplyRedirection(shellInput, command, proc.StandardOutput.BaseStream, proc.StandardError.BaseStream);
      shellInput.OutputTasks.Add(outputTask);
    }
    catch (System.ComponentModel.Win32Exception)
    {
      // This is the specific fix for the tester's expected output
      Console.WriteLine($"{command.Name}: command not found");

      // Drain any existing pipe to prevent the next command from hanging
      if (shellInput.LastPipeReadStream != null)
      {
        await shellInput.LastPipeReadStream.DisposeAsync();
        shellInput.LastPipeReadStream = null;
      }
    }
  }

  public static async Task ApplyRedirection(ShellContext shellInput, Command command, Stream stdoutSource, Stream? stderrSource = null)
  {
    // FIX: Internal commands should use Stream.Null (readable but empty) 
    // instead of the Write-Only Console.Error stream.
    stderrSource ??= Stream.Null;

    // --- 1. HANDLE STDOUT ---
    if (command.StdoutTarget != "Console")
    {
      FileMode mode = command.OutputType == OutputType.Append ? FileMode.Append : FileMode.Create;
      var fileStream = new FileStream(command.StdoutTarget, mode, FileAccess.Write, FileShare.Read);
      shellInput.OutputTasks.Add(CopyStreamAsync(stdoutSource, fileStream));
    }
    else if (shellInput.Commands.Last().Index != command.Index)
    {
      var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
      shellInput.LastPipeReadStream = pipeServer;
      shellInput.OutputTasks.Add(CopyStreamAsync(stdoutSource, pipeServer));
    }
    else
    {
      shellInput.OutputTasks.Add(CopyStreamAsync(stdoutSource, Console.OpenStandardOutput()));
    }

    // --- 2. HANDLE STDERR ---
    if (command.SterrTarget != null && command.SterrTarget != "Console")
    {
      FileMode mode = command.OutputType == OutputType.Append ? FileMode.Append : FileMode.Create;
      var fileStream = new FileStream(command.SterrTarget, mode, FileAccess.Write, FileShare.Read);
      shellInput.OutputTasks.Add(CopyStreamAsync(stderrSource, fileStream));
    }
    else
    {
      // For external commands, this is the actual proc.StandardError.
      // For internal commands, this is now Stream.Null, which won't crash.
      shellInput.OutputTasks.Add(CopyStreamAsync(stderrSource, Console.OpenStandardError()));
    }
  }

  private static async Task CopyStreamAsync(Stream source, Stream destination)
  {
    try
    {
      // Copy the data
      await source.CopyToAsync(destination);
      await destination.FlushAsync();
    }
    catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
    {
      // Standard shell behavior: If a pipe is closed early (e.g., 'head' finishes)
      // we just stop copying instead of crashing.
    }
    finally
    {
      // 1. Always dispose the source (it's a process stdout or internal pipe)
      await source.DisposeAsync();

      // 2. ONLY dispose destination if it's a File or an Anonymous Pipe.
      // NEVER dispose Console.OpenStandardOutput() or Error.
      if (destination is FileStream || destination is AnonymousPipeServerStream || destination is AnonymousPipeClientStream)
      {
        await destination.DisposeAsync();
      }
    }
  }
}
