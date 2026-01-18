class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");
            
            string command = Console.ReadLine() ?? "";

            if (command == "exit")
            {
                break;
            }

            if (command != "")
            {
                Console.WriteLine($"{command}: command not found");
            }
        }
    }
}
