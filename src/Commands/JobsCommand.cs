using src.Classes;

namespace src.Commands;

public static class JobsCommand
{
  public static Stream Run(ShellContext shellContext, Command _) =>
      InternalCommand.CreateStream(async (writer) =>
      {
        shellContext.BackgroundJobs.RemoveAll(job => job.Process.HasExited);

        var runningJobs = shellContext.BackgroundJobs
          .Where(job => !job.Process.HasExited)
          .OrderBy(job => job.JobNumber)
          .ToList();

        if (runningJobs.Count == 0)
          return;

        int currentJobNumber = runningJobs[^1].JobNumber;

        foreach (var job in runningJobs)
        {
          string marker = job.JobNumber == currentJobNumber ? "+" : "-";
          await writer.WriteLineAsync($"[{job.JobNumber}]{marker}  {job.Status,-24}{job.CommandText}");
        }
      });
}
