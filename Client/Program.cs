using System.IO;
using System;
using System.Net;

namespace Client
{
    class Program
    {
        static IPAddress ServerIpAddress;
        static int ServerPort = 0;

        static string ClientToken;
        static DateTime ClientDateTimeConnection;

        static void Main(string[] args)
        {
            OnSettings();
            while (true) 
                SetCommand();
        }

        static void SetCommand()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            string Command = Console.ReadLine();

            if (Command == "/config")
            {
                File.Delete(Directory.GetCurrentDirectory() + "/.config");
                OnSettings();
            }
            else if (Command == "/connect") ;
            else if (Command == "/status") GetStatus();
            else if (Command == "/help") Help();
        }

        static void GetStatus()
        {
            int Duration = (int)DateTime.Now.Subtract(ClientDateTimeConnection).TotalSeconds;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Client: {ClientToken}, time connection: {ClientDateTimeConnection.ToString("HH:mm:ss dd.MM")}, " + 
                $"duration: {Duration}");
        }

        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Commands to the server: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n/config");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - set initial settings");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/connect");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - connection to the server");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/status");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - show list users");
        }

        static void OnSettings()
        {
            string Path = Directory.GetCurrentDirectory() + "/.config";
            string IpAddress = "";

            if (File.Exists(Path))
            {
                StreamReader streamReader = new StreamReader(Path);
                IpAddress = streamReader.ReadLine();
                ServerIpAddress = IPAddress.Parse(IpAddress);
                ServerPort = int.Parse(streamReader.ReadLine());
                streamReader.Close();

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Server address: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(IpAddress);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Server port: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(ServerPort.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Please provide the IP address if the licence server: ");
                Console.ForegroundColor = ConsoleColor.Green;
                IpAddress = Console.ReadLine();
                ServerIpAddress = IPAddress.Parse(IpAddress);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Please specify the licence server port: ");
                Console.ForegroundColor = ConsoleColor.Green;
                ServerPort = int.Parse(Console.ReadLine());

                StreamWriter streamWriter = new StreamWriter(Path);
                streamWriter.WriteLine(IpAddress); 
                streamWriter.WriteLine(ServerPort.ToString());
                streamWriter.Close();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("To change, write the command: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/config");
            Console.WriteLine(Path);
        }


    }
}
