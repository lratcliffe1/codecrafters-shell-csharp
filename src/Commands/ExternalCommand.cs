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
      proc.StartInfo.ArgumentList.Add(arg);

    try
    {
      proc.Start();
      shellInput.Processes.Add(proc);

      // If we have a pipeline read-stream from the previous command, feed it into stdin.
      if (shellInput.LastPipeReadStream != null)
      {
        shellInput.OutputTasks.Add(PumpToProcessStdinAsync(shellInput.LastPipeReadStream, proc));
        shellInput.LastPipeReadStream = null;
      }

      await ApplyRedirection(shellInput, command, proc.StandardOutput.BaseStream, proc.StandardError.BaseStream);
    }
    catch (System.ComponentModel.Win32Exception)
    {
      Console.WriteLine($"{command.Name}: command not found");

      if (shellInput.LastPipeReadStream != null)
      {
        await shellInput.LastPipeReadStream.DisposeAsync();
        shellInput.LastPipeReadStream = null;
      }
    }
  }

  public static async Task ApplyRedirection(
    ShellContext shellInput,
    Command command,
    Stream stdoutSource,
    Stream? stderrSource = null)
  {
    stderrSource ??= Stream.Null;

    bool isLastCommand = ReferenceEquals(command, shellInput.Commands[^1]);

    // --- STDOUT ---
    if (command.StdoutTarget != "Console")
    {
      FileMode mode = command.OutputType == OutputType.Append ? FileMode.Append : FileMode.Create;
      var fileStream = new FileStream(command.StdoutTarget, mode, FileAccess.Write, FileShare.Read);
      shellInput.OutputTasks.Add(CopyStreamAsync(stdoutSource, fileStream, leaveDestinationOpen: false));
    }
    else if (!isLastCommand)
    {
      // Pipe stdout to the next command by passing the stream directly.
      shellInput.LastPipeReadStream = stdoutSource;
    }
    else
    {
      // Last command prints to console (DO NOT close console stream)
      shellInput.OutputTasks.Add(CopyStreamAsync(stdoutSource, Console.OpenStandardOutput(), leaveDestinationOpen: true));
    }

    // --- STDERR ---
    if (command.SterrTarget != null && command.SterrTarget != "Console")
    {
      FileMode mode = command.OutputType == OutputType.Append ? FileMode.Append : FileMode.Create;
      var fileStream = new FileStream(command.SterrTarget, mode, FileAccess.Write, FileShare.Read);
      shellInput.OutputTasks.Add(CopyStreamAsync(stderrSource, fileStream, leaveDestinationOpen: false));
    }
    else
    {
      shellInput.OutputTasks.Add(CopyStreamAsync(stderrSource, Console.OpenStandardError(), leaveDestinationOpen: true));
    }

    await Task.CompletedTask;
  }

  private static async Task CopyStreamAsync(Stream source, Stream destination, bool leaveDestinationOpen)
  {
    try
    {
      await source.CopyToAsync(destination);
      await destination.FlushAsync();

      if (destination is AnonymousPipeServerStream pipeServer && OperatingSystem.IsWindows())
        pipeServer.WaitForPipeDrain();
    }
    catch (IOException)
    {
      // Broken pipe is normal for commands like `head`
    }
    finally
    {
      await source.DisposeAsync();

      if (!leaveDestinationOpen)
      {
        if (destination is FileStream || destination is PipeStream)
          await destination.DisposeAsync();
        else
          destination.Dispose();
      }
    }
  }

  private static async Task PumpToProcessStdinAsync(Stream source, Process proc)
  {
    try
    {
      await source.CopyToAsync(proc.StandardInput.BaseStream);
      await proc.StandardInput.FlushAsync();
    }
    catch (IOException)
    {
      // Consumer may exit early
    }
    finally
    {
      await source.DisposeAsync();
      try { proc.StandardInput.Close(); } catch { }
    }
  }
}
