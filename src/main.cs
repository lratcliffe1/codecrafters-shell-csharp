using System.Diagnostics;

class Program
{
    static void Main()
    {
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
                    Console.WriteLine(string.Join(" ", shellInput.Parameters));
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

    static void PrintType(ShellInput input)
    {
        if (input.Parameters == "exit" || input.Parameters == "echo" || input.Parameters == "type")
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

    class ShellInput
    {
        public required string Input { get; set; }

        public required string Command { get; set; }

        public required string Parameters { get; set; }
    }
}
