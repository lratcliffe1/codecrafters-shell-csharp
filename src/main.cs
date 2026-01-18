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
                case Command.Exit:
                    return;
                case Command.Echo:
                    Console.WriteLine(string.Join(" ", shellInput.Parameters));
                    break;
                case Command.Type:
                    PrintType(shellInput);
                    break;
                default:
                    Console.WriteLine($"{shellInput.Input}: command not found");
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
            Input = input, 
            Command = GetCommandType(parts[0]), 
            Parameters = parts.Length > 1 ? parts[1] : string.Empty,
        };
    }

    static Command GetCommandType(string command)
    {
        List<string> commandList = command.Split(" ").ToList();

        string baseCommand = commandList?.First() ?? "";
            
        return Enum.TryParse(baseCommand, true, out Command type) ? type : Command.None;
    }

    static void PrintType(ShellInput input)
    {
        Command typeCommand = GetCommandType(input.Parameters);

        if (typeCommand == Command.None)
        {
            string? executablePath = FindExecutablePath(input.Parameters);

            if (executablePath != null)
                Console.WriteLine($"{input.Parameters} is {executablePath}");
            else
                Console.WriteLine($"{input.Parameters}: not found");
        }
        else 
            Console.WriteLine($"{input.Parameters} is a shell builtin");
    }

    static string? FindExecutablePath(string fileName)
    {
        string paths = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        char separator = Path.PathSeparator;

        foreach (string p in paths.Split(separator))
        {
            if (!Directory.Exists(p))
                continue;

            string[] files = Directory.GetFiles(p);

            foreach (string file in files)
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
        }

        return null;
    }

    class ShellInput
    {
        public required string Input { get; set; }

        public required Command Command { get; set; }

        public required string Parameters { get; set; }
    }

    enum Command
    {
        None = 0,
        Echo = 1, 
        Exit = 2,
        Type = 3,  
    }
}
