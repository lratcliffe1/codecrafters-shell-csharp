using src.Classes;

namespace src.Helpers;

public static class FileExecuter
{
  public static string? FindExecutablePath(string fileName)
  {
    string paths = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    char separator = Path.PathSeparator;

    foreach (string path in paths.Split(separator))
    {
      if (!Directory.Exists(path))
        continue;

      string[] files = Directory.GetFiles(path);

      foreach (string file in files)
      {
        try
        {
          if (!File.Exists(file))
            continue;
      
          UnixFileMode mode = File.GetUnixFileMode(file);

          bool isExecutable = mode.HasFlag(UnixFileMode.UserExecute) || 
            mode.HasFlag(UnixFileMode.GroupExecute) || 
            mode.HasFlag(UnixFileMode.OtherExecute);

          if (isExecutable && Path.GetFileName(file) == fileName)
          {
            return file;
          }
        }
        catch {}
      }
    }

    return null;
  }

  public static async Task WriteToFile(string path, string? contents)
  {
    string? directoryPath = Path.GetDirectoryName(path);

    if (!string.IsNullOrEmpty(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    await File.WriteAllTextAsync(path, contents);
  }
}
