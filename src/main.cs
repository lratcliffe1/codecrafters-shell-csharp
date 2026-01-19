using System.Diagnostics;
using classes;

class Program
{
    static List<string> builtInCommands = ["exit", "echo", "type", "pwd", "cd"];

    static void Main()
    {        
        string workingDirectory = Directory.GetCurrentDirectory();

        while (true)
        {
            ShellInput shellInput = GetCommandFromUser();

            if (shellInput.Input == "")
                continue;

            switch (shellInput.Command)
            {
                case "exit":
                    return;
                case "echo":
                    Console.WriteLine(shellInput.Parameters);
                    break;
                case "pwd":
                    PrintWorkingDirectory(workingDirectory);
                    break;
                case "cd":
                    ChangeDirectory(shellInput, ref workingDirectory);
                    break;
                case "type":
                    PrintType(shellInput);
                    break;
                default:
                    RunExternalProgram(shellInput);
                    break;
            }
        }
    }

    static ShellInput GetCommandFromUser()
    {
        Console.Write("$ ");
            
        string input = Console.ReadLine() ?? "";

        string[] parts = input.Split(" ", 2);

        return new ShellInput { 
            Input = input.ToLower(), 
            Command = parts[0].ToLower(), 
            Parameters = parts.Length > 1 ? parts[1] : string.Empty,
        };
    }

    static void PrintWorkingDirectory(string workingDirectory)
    {
        Console.WriteLine(workingDirectory);
    }

    static void ChangeDirectory(ShellInput input, ref string workingDirectory)
    {
        string home = Environment.GetEnvironmentVariable("HOME") ?? string.Empty;

        string[] parts = input.Parameters.Split("/");

        if (parts.Length > 1 && parts[^1] == "")
        {
            parts = parts[0..^1];
        }

        string targetWorkingDirectory = workingDirectory;

        foreach (string part in parts)
        {
            switch (part)
            {
                case "~":
                    targetWorkingDirectory = home;
                    break;
                case "":
                    targetWorkingDirectory = "";
                    break;
                case ".":
                    targetWorkingDirectory = workingDirectory;
                    break;
                case "..":
                    targetWorkingDirectory = targetWorkingDirectory.Substring(0, targetWorkingDirectory.LastIndexOf('/'));
                    break;
                default:
                    targetWorkingDirectory += "/" + part;
                    break;
            }
        }

        if (!Directory.Exists(targetWorkingDirectory))
        {
            Console.WriteLine($"cd: {targetWorkingDirectory}: No such file or directory");
            return;
        }
        workingDirectory = targetWorkingDirectory;
    }

    static void PrintType(ShellInput input)
    {
        if (builtInCommands.Contains(input.Parameters))
        {
            Console.WriteLine($"{input.Parameters} is a shell builtin");
            return;
        }
        
        string? executablePath = FindExecutablePath(input.Parameters);

        if (executablePath != null)
            Console.WriteLine($"{input.Parameters} is {executablePath}");
        else
            Console.WriteLine($"{input.Parameters}: not found");
    }

    static void RunExternalProgram(ShellInput input)
    {
        string? executablePath = FindExecutablePath(input.Command);

        if (executablePath != null)
        {
            Process process = Process.Start(input.Command, input.Parameters);
            process.WaitForExit();
        }
        else
            Console.WriteLine($"{input.Input}: command not found");
    }

    static string? FindExecutablePath(string fileName)
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
}
