using src.Classes;

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

        int currentJobNumber = jobs[^1].JobNumber;
        int? previousJobNumber = jobs.Count > 1 ? jobs[^2].JobNumber : null;

        foreach (var job in jobs)
        {
          bool isDone = job.Process.HasExited;
          string marker =
            job.JobNumber == currentJobNumber ? "+" :
            job.JobNumber == previousJobNumber ? "-" : " ";
          string status = isDone ? "Done" : "Running";
          string commandText = isDone ? TrimBackgroundSuffix(job.CommandText) : job.CommandText;
          await writer.WriteLineAsync($"[{job.JobNumber}]{marker}  {status,-24}{commandText}");
        }

        shellContext.BackgroundJobs.RemoveAll(job => job.Process.HasExited);
      });

  private static string TrimBackgroundSuffix(string commandText)
  {
    return commandText.EndsWith(" &") ? commandText[..^2] : commandText;
  }
}
