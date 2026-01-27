using System.Runtime.InteropServices;

namespace src.Helpers;

public static class FileExecuter
{
  public static string? FindExecutablePath(string fileName)
  {
    return GetPathSegments()
      .SelectMany(Directory.EnumerateFiles)
      .Where(IsExecutable)
      .FirstOrDefault(file => Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase));
  }

  public static string[] FindExecutablesAtPath(string? optionalPath = null)
  {
    return GetPathSegments(optionalPath)
      .SelectMany(Directory.EnumerateFiles)
      .Where(IsExecutable)
      .Select(Path.GetFileName)
      .OfType<string>()
      .Distinct()
      .ToArray();
  }

  private static IEnumerable<string> GetPathSegments(string? customPath = null)
  {
    string paths = customPath ?? Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    return paths.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Where(Directory.Exists);
  }

  private static bool IsExecutable(string file)
  {
    try
    {
      if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        UnixFileMode mode = File.GetUnixFileMode(file);
        return mode.HasFlag(UnixFileMode.UserExecute) || mode.HasFlag(UnixFileMode.GroupExecute) || mode.HasFlag(UnixFileMode.OtherExecute);
      }

      string extension = Path.GetExtension(file).ToLowerInvariant();
      return extension == ".exe" || extension == ".bat" || extension == ".cmd" || extension == ".com";
    }
    catch
    {
      return false;
    }
  }
}
