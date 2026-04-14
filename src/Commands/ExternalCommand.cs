using System.Diagnostics;
using System.IO.Pipes;
using src.Classes;

namespace src.Commands;

public class ExternalCommand
{
  public static async Task Run(ShellContext shellInput, Command command)
  {
    bool isLastCommand = ReferenceEquals(command, shellInput.Commands[^1]);
    bool inheritsTerminalOutput =
      command.IsBackground
      && isLastCommand
      && shellInput.LastPipeReadStream == null
      && command.StdoutTarget == "Console"
      && (command.SterrTarget == null || command.SterrTarget == "Console");

    var proc = new Process();
    proc.StartInfo = new ProcessStartInfo(command.Name)
    {
      RedirectStandardInput = !inheritsTerminalOutput,
      RedirectStandardOutput = !inheritsTerminalOutput,
      RedirectStandardError = !inheritsTerminalOutput,
      UseShellExecute = false
    };

    foreach (var arg in command.Args)
      proc.StartInfo.ArgumentList.Add(arg);

    try
    {
      proc.Start();
      if (command.IsBackground)
      {
        shellInput.BackgroundProcesses.Add(proc);
        if (command.JobNumber != null)
        {
          shellInput.BackgroundJobs.Add(new BackgroundJob
          {
            JobNumber = command.JobNumber.Value,
            ProcessId = proc.Id,
            CommandText = command.OriginalCommandText ?? $"{command.Name} {string.Join(" ", command.Args)} &".Trim(),
            Status = "Running",
            Process = proc
          });
        }

        if (command.JobNumber != null)
          Console.WriteLine($"[{command.JobNumber}] {proc.Id}");
      }
      else
      {
        shellInput.Processes.Add(proc);
      }

      // If we have a pipeline read-stream from the previous command, feed it into stdin.
      if (shellInput.LastPipeReadStream != null)
      {
        RegisterOutputTask(shellInput, command, PumpToProcessStdinAsync(shellInput.LastPipeReadStream, proc));
        shellInput.LastPipeReadStream = null;
      }

      if (!inheritsTerminalOutput)
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
    // STDOUT
    if (command.StdoutTarget != "Console")
    {
      FileMode mode = command.OutputType == OutputType.Append ? FileMode.Append : FileMode.Create;
      var fileStream = new FileStream(command.StdoutTarget, mode, FileAccess.Write, FileShare.Read);
      RegisterOutputTask(shellInput, command, CopyStreamAsync(stdoutSource, fileStream, leaveDestinationOpen: false));
    }
    else if (!isLastCommand)
    {
      // Pipe stdout to the next command by passing the stream directly.
      shellInput.LastPipeReadStream = stdoutSource;
    }
    else
    {
      // Last command prints to console (DO NOT close console stream)
      RegisterOutputTask(shellInput, command, CopyStreamAsync(stdoutSource, Console.OpenStandardOutput(), leaveDestinationOpen: true));
    }

    // STDERR
    if (command.SterrTarget != null && command.SterrTarget != "Console")
    {
      FileMode mode = command.OutputType == OutputType.Append ? FileMode.Append : FileMode.Create;
      var fileStream = new FileStream(command.SterrTarget, mode, FileAccess.Write, FileShare.Read);
      RegisterOutputTask(shellInput, command, CopyStreamAsync(stderrSource, fileStream, leaveDestinationOpen: false));
    }
    else
    {
      RegisterOutputTask(shellInput, command, CopyStreamAsync(stderrSource, Console.OpenStandardError(), leaveDestinationOpen: true));
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
    catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException || ex is UnauthorizedAccessException)
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
      var buffer = new byte[8192];
      long totalBytes = 0;

      while (true)
      {
        int read = await source.ReadAsync(buffer);
        if (read == 0)
        {
          break;
        }

        totalBytes += read;

        await proc.StandardInput.BaseStream.WriteAsync(buffer.AsMemory(0, read));
      }

      await proc.StandardInput.FlushAsync();
    }
    catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException || ex is UnauthorizedAccessException)
    {
      // Consumer may exit early
    }
    finally
    {
      await source.DisposeAsync();
      try { proc.StandardInput.Close(); } catch { }
    }
  }

  private static void RegisterOutputTask(ShellContext shellInput, Command command, Task task)
  {
    if (command.IsBackground)
      shellInput.BackgroundOutputTasks.Add(task);
    else
      shellInput.OutputTasks.Add(task);
  }
}
