namespace SpiritEye
{
    class Program
    {
        // sudo nmap -sV --open 16.163.13.0/24 -oX result.xml
        static void Main(string[] args)
        {
            CommandLine.HandleCommandLine(args);
        }
    }
}
