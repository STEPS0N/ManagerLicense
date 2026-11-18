using System.IO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static IPAddress ServerIpAddress;
        static int ServerPort = 0;
        static string ClientName;

        static string ClientToken;
        static DateTime ClientDateTimeConnection;

        static void Main(string[] args)
        {
            OnSettings();
            
            Thread tCheckToken = new Thread(CheckToken);
            tCheckToken.Start();

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
            else if (Command == "/connect") ConnectServer();
            else if (Command == "/status") GetStatus();
            else if (Command == "/help") Help();
        }

        static void ConnectServer()
        {
            IPEndPoint EndPoint = new IPEndPoint(ServerIpAddress, ServerPort);
            Socket Socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                Socket.Connect(EndPoint);
            }
            catch (Exception ex) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
            }

            if (Socket.Connected)
            {
                Socket.Send(Encoding.UTF8.GetBytes("/token " + ClientName));

                byte[] Bytes = new byte[10485760];
                int ByteRec = Socket.Receive(Bytes);

                string Response = Encoding.UTF8.GetString(Bytes, 0, ByteRec);
                if (Response == "/limit")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There is not enough space on the license server");
                }
                else if (Response == "/blacklisted")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Клиент находится в чёрном списке");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Connection to server successful");
                    ClientToken = Response;
                    ClientDateTimeConnection = DateTime.Now;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Recieved connection token: " + ClientToken);
                }
            }
        }

        static void CheckToken()
        {
            while (true)
            {
                if (!String.IsNullOrEmpty(ClientToken))
                {
                    IPEndPoint EndPoint = new IPEndPoint(ServerIpAddress, ServerPort);
                    Socket Socket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);

                    try
                    {
                        Socket.Connect(EndPoint);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: " + ex.Message);
                    }

                    if (Socket.Connected)
                    {
                        Socket.Send(Encoding.UTF8.GetBytes(ClientToken));

                        byte[] Bytes = new byte[10485760];
                        int ByteRec = Socket.Receive(Bytes);

                        string Response = Encoding.UTF8.GetString(Bytes, 0, ByteRec);
                        if (Response == "/disconnect")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The client is disconnected from the server");
                            ClientToken = String.Empty;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        static void GetStatus()
        {
            int Duration = (int)DateTime.Now.Subtract(ClientDateTimeConnection).TotalSeconds;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Client: {ClientName} ({ClientToken}), time connection: {ClientDateTimeConnection.ToString("HH:mm:ss dd.MM")}, " + 
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
                ClientName = streamReader.ReadLine();
                streamReader.Close();

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Server address: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(IpAddress);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Server port: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(ServerPort.ToString());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Сlient name: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(ClientName.ToString());
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

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Please enter your client name: ");
                Console.ForegroundColor = ConsoleColor.Green;
                ClientName = Console.ReadLine();

                StreamWriter streamWriter = new StreamWriter(Path);
                streamWriter.WriteLine(IpAddress); 
                streamWriter.WriteLine(ServerPort.ToString());
                streamWriter.WriteLine(ClientName);
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
