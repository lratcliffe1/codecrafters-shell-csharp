using System.Runtime.InteropServices;
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

          bool isExecutable = false;

          if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
          {
              UnixFileMode mode = File.GetUnixFileMode(file);
              isExecutable = mode.HasFlag(UnixFileMode.UserExecute) || mode.HasFlag(UnixFileMode.GroupExecute) || mode.HasFlag(UnixFileMode.OtherExecute);
          }
          else
          {
              string extension = Path.GetExtension(file).ToLowerInvariant();
              isExecutable = extension == ".exe" || extension == ".bat" || extension == ".cmd" || extension == ".com";
          }

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

  public static async Task WriteToFile(string path, string? contents, OutputType? outputType)
  {
    string? directoryPath = Path.GetDirectoryName(path);

    if (!string.IsNullOrEmpty(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    if (outputType == OutputType.Redirect)
      await File.WriteAllTextAsync(path, contents);
    else if (outputType == OutputType.Append)
      await File.AppendAllTextAsync(path, Environment.NewLine + contents);
  }
}
