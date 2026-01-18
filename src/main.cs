class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");
            
            string command = Console.ReadLine() ?? "";

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
}
