using src.Classes;
using src.Helpers;

namespace src.Commands;

public static class JobsCommand
{
  public static Stream Run(ShellContext shellContext, Command _) =>
      InternalCommand.CreateStream(async (writer) =>
      {
        var jobs = shellContext.BackgroundJobs
          .OrderBy(job => job.JobNumber)
          .ToList();
        if (jobs.Count == 0)
          return;

        foreach (var job in jobs)
        {
          string marker = GetMarker(job.JobNumber, jobs);
          bool isDone = job.Process.HasExited;
          string status = isDone ? "Done" : "Running";
          string commandText = isDone ? TrimBackgroundSuffix(job.CommandText) : job.CommandText;
          await writer.WriteLineAsync($"[{job.JobNumber}]{marker}  {status,-24}{commandText}");
        }

        JobReaper.RemoveCompletedJobs(shellContext);
      });

  private static string GetMarker(int jobNumber, List<BackgroundJob> jobs)
  {
    int currentJobNumber = jobs[^1].JobNumber;
    int? previousJobNumber = jobs.Count > 1 ? jobs[^2].JobNumber : null;

    return jobNumber == currentJobNumber ? "+" :
      jobNumber == previousJobNumber ? "-" : " ";
  }

  private static string TrimBackgroundSuffix(string commandText)
  {
    return commandText.EndsWith(" &") ? commandText[..^2] : commandText;
  }
}
