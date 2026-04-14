using src.Classes;

namespace src.Helpers;

public static class JobReaper
{
  public static async Task ReapCompletedJobs(ShellContext shellContext, Func<string, Task> writeLineAsync)
  {
    var jobs = shellContext.BackgroundJobs
      .OrderBy(job => job.JobNumber)
      .ToList();

    if (jobs.Count == 0)
      return;

    var completedJobs = jobs
      .Where(job => job.Process.HasExited)
      .ToList();

    foreach (var job in completedJobs)
    {
      string marker = GetMarker(job.JobNumber, jobs);
      string commandText = TrimBackgroundSuffix(job.CommandText);
      await writeLineAsync($"[{job.JobNumber}]{marker}  {"Done",-24}{commandText}");
    }

    RemoveCompletedJobs(shellContext, completedJobs);
  }

  public static void RemoveCompletedJobs(ShellContext shellContext)
  {
    var completedJobs = shellContext.BackgroundJobs
      .Where(job => job.Process.HasExited)
      .ToList();

    RemoveCompletedJobs(shellContext, completedJobs);
  }

  private static void RemoveCompletedJobs(ShellContext shellContext, List<BackgroundJob> completedJobs)
  {
    foreach (var completedJob in completedJobs)
    {
      try { completedJob.Process.Dispose(); } catch { }
      shellContext.BackgroundJobs.Remove(completedJob);
    }
  }

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
