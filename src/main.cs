class Program
{
    enum Types
    {
        None = 0,
        echo = 1, 
        exit = 2,
        type = 3    
    }

    static void Main()
    {
        while (true)
        {
            string command = GetCommandFromUser();

            Types commandtype = GetCommandType(command);

            Console.Write(commandtype);

            List<string> commandList = command.Split(" ").ToList();

            string baseCommand = commandList?.First() ?? "";
            List<string> restOfCommand = commandList[1..]?.ToList() ?? [];

            switch (baseCommand)
            {
                case "exit":
                    return;
                case "echo":
                    Console.WriteLine(string.Join(" ", restOfCommand));
                    break;
                case "":
                    break;
                default:
                    Console.WriteLine($"{command}: command not found");
                    break;
            }
        }
    }

    static string GetCommandFromUser()
    {
        Console.Write("$ ");
            
        return Console.ReadLine() ?? "";
    }

    static Types GetCommandType(string command)
    {
        List<string> commandList = command.Split(" ").ToList();

        string baseCommand = commandList?.First() ?? "None";
            
        return Enum.TryParse(Types, baseCommand);
    }
}
