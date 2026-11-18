using Server.Classes;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace Server
{
    class Program
    {
        static IPAddress ServerIpAddress;
        static int ServerPort;
        static int MaxClient;
        static int Duration;
        static List<Client> AllClients = new List<Client>();
        static List<string> BlackList = new List<string>();

        static void Main(string[] args)
        {
            OnSettings();

            Thread tListener = new Thread(ConnectServer);
            tListener.Start();

            Thread tDisconnect = new Thread(CheckDisconnectClient);
            tDisconnect.Start();

            while (true)
                SetCommand();
        }

        static void CheckDisconnectClient()
        {
            while (true)
            {
                for (int iClient = 0; iClient < AllClients.Count; iClient++)
                {
                    int ClientDuration = (int)DateTime.Now.Subtract(AllClients[iClient].DataConnect).TotalSeconds;

                    if (ClientDuration > Duration)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Клиент: {AllClients[iClient].Token} отключился от сервера из-за тайм-аута");
                        AllClients.RemoveAt(iClient);
                    }
                }

                Thread.Sleep(1000);
            }
        }

        static void BlackListClient(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Укажите имя клиента: /blacklist NAME");
                return;
            }

            if (BlackList.Contains(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Клиент '{name}' уже в чёрном списке");
                return;
            }

            BlackList.Add(name);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Клиент '{name}' добавлен в чёрный список");

            int Removed = AllClients.RemoveAll(x => x.Name == name);
            if (Removed > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Отключено активных сессий: {Removed}");
            }
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
            else if (Command.StartsWith("/blacklist "))
                BlackListClient(Command.Replace("/blacklist ", "").Trim());
            else if (Command == "/status")
                GetStatus();
            else if (Command == "/help")
                Help();
        }

        static void ConnectServer()
        {
            IPEndPoint EndPoint = new IPEndPoint(ServerIpAddress, ServerPort);
            Socket SocketListener = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            SocketListener.Bind(EndPoint);
            SocketListener.Listen(10);

            while (true)
            {
                Socket Handler = SocketListener.Accept();

                byte[] Bytes = new byte[10485760];
                int ByteRec = Handler.Receive(Bytes);

                string Message = Encoding.UTF8.GetString(Bytes, 0, ByteRec);
                string Response = SetCommandClient(Message);

                Handler.Send(Encoding.UTF8.GetBytes(Response));
            }
        }

        static string SetCommandClient(string Command)
        {
            if (Command.StartsWith("/token"))
            {
                string clientName = Command.Replace("/token", "").Trim();

                if (BlackList.Contains(clientName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Отказ: клиент '{clientName}' в чёрном списке");
                    return "/blacklisted";
                }

                if (AllClients.Count < MaxClient)
                {
                    Client newClient = new Client();
                    newClient.Name = clientName;
                    AllClients.Add(newClient);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Новое подключение клиента: {newClient.Token} (имя: {clientName})");

                    return newClient.Token;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("На сервере лицензий недостаточно места");
                    return "/limit";
                }
            }
            else
            {
                Client Client = AllClients.Find(x => x.Token == Command);
                return Client != null ? "/connect" : "/disconnect";
            }
        }

        static void DisconnectServer(string command)
        {
            try
            {
                string Token = command.Replace("/disconnect", "").Trim();
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
            Console.WriteLine($"В чёрном списке: {BlackList.Count}");

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
            Console.Write("/blacklist");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" - добавить клиента в чёрный список");

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

                Console.Write(Path);
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
