using System.Runtime.InteropServices;
using src.Classes;

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

  public static async Task WriteToFile(string path, string? contents, OutputType? outputType)
  {
    string? directoryPath = Path.GetDirectoryName(path);

    if (!string.IsNullOrEmpty(directoryPath))
        Directory.CreateDirectory(directoryPath);

    if (outputType == OutputType.Redirect)
      await File.WriteAllTextAsync(path, contents);
    else if (outputType == OutputType.Append)
    {
      bool hasContent = File.Exists(path) && new FileInfo(path).Length > 0;
    
      if (hasContent)
        await File.AppendAllTextAsync(path, Environment.NewLine + contents);
      else 
        await File.AppendAllTextAsync(path, contents);
    }
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
