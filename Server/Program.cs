using Server.Classes;
using System;
using System.Net;

namespace Server
{
    class Program
    {
        static IPAddress ServerIpAddress;
        static int ServerPort;
        static int MaxClient;
        static int Duration;
        static List<Client> AllClients = new List<Client>();

        static void Main(string[] args)
        {

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
            else if (Command.Contains("/disconnect"))
                DisconnectServer(Command);
            else if (Command == "/status")
                GetStatus();
            else if (Command == "/help")
                Help();
        }

        static void DisconnectServer(string command)
        {
            try
            {
                string Token = command.Replace("/disconnect", "");
                Client DisconnectClient = AllClients.Find(x => x.Token == Token);
                AllClients.Remove(DisconnectClient);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Клиент: {Token} отключился от сервера");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Что-то пошло не так: " + ex.Message);
            }
        }

        static void GetStatus()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Количество клиентов: {AllClients.Count}");

            foreach (Client Client in AllClients)
            {
                int Duration = (int)DateTime.Now.Subtract(Client.DataConnect).TotalSeconds;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Клиент: {Client.Token}, время подключения: {Client.DataConnect.ToString("HH:mm:ss dd.MM")}, " +
                    $" продолжительность: {Duration}");
            }
        }

        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Команды для сервера: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/config");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - задать первоначальные настройки");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/disconnect");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - отключить пользователей от сервера");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/status");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - показать список пользователей");
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
                MaxClient = int.Parse(streamReader.ReadLine());
                Duration = int.Parse(streamReader.ReadLine());
                streamReader.Close();

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("IP адрес сервера: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(IpAddress);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Порт: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(ServerPort.ToString());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Максимальное количество клиентов: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(MaxClient .ToString());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Срок действия токена: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Duration.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Пожалуйста, укажите IP-адрес сервера лицензий: ");
                Console.ForegroundColor = ConsoleColor.Green;
                IpAddress = Console.ReadLine();
                ServerIpAddress = IPAddress.Parse(IpAddress);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Пожалуйста, укажите порт сервера лицензий: ");
                Console.ForegroundColor = ConsoleColor.Green;
                ServerPort = int.Parse(Console.ReadLine());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Пожалуйста, укажите максимальное количество клиентов: ");
                Console.ForegroundColor = ConsoleColor.Green;
                MaxClient = int.Parse(Console.ReadLine());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Укажите срок действия токена: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Duration = int.Parse(Console.ReadLine());

                StreamWriter streamWriter = new StreamWriter(Path);
                streamWriter.WriteLine(IpAddress);
                streamWriter.WriteLine(ServerPort.ToString());
                streamWriter.WriteLine(MaxClient.ToString());
                streamWriter.WriteLine(Duration.ToString());
                streamWriter.Close();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Чтобы изменить, напишите команду: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/config");
        }
    }
}
